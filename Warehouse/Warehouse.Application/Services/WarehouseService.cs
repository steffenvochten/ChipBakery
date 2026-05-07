using ChipBakery.Shared;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Warehouse.Application.DTOs;
using Warehouse.Application.Interfaces;
using Warehouse.Application.Mapping;
using Warehouse.Domain.Entities;
using Warehouse.Domain.Events;
using Warehouse.Domain.Exceptions;
using Warehouse.Domain.Interfaces;

namespace Warehouse.Application.Services;

public class WarehouseService(
    IWarehouseRepository repository,
    IRecipeRepository recipeRepository,
    IEventPublisher eventPublisher,
    IValidator<CreateWarehouseItemRequest> createValidator,
    IValidator<UpdateWarehouseItemRequest> updateValidator,
    IValidator<DeductStockRequest> deductValidator,
    ILogger<WarehouseService> logger) : IWarehouseService
{
    public async Task<List<WarehouseItemDto>> GetAllItemsAsync(CancellationToken ct = default)
    {
        var items = await repository.GetAllAsync(ct);
        return items.ToDtoList();
    }

    public async Task<WarehouseItemDto> GetItemByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await repository.GetByIdAsync(id, ct);
        if (item == null) throw new WarehouseItemNotFoundException(id);
        return item.ToDto();
    }

    public async Task<WarehouseItemDto> CreateItemAsync(CreateWarehouseItemRequest request, CancellationToken ct = default)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        var item = new Warehouse.Domain.Entities.WarehouseItem
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Quantity = request.Quantity,
            Unit = request.Unit
        };

        await repository.AddAsync(item, ct);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Created warehouse item {ItemName} ({ItemId})", item.Name, item.Id);
        await eventPublisher.PublishAsync(new WarehouseItemCreatedEvent(item.Id, item.Name, item.Quantity, item.Unit), ct);

        return item.ToDto();
    }

    public async Task<WarehouseItemDto> UpdateItemAsync(Guid id, UpdateWarehouseItemRequest request, CancellationToken ct = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);

        var item = await repository.GetByIdAsync(id, ct);
        if (item == null) throw new WarehouseItemNotFoundException(id);

        item.Name = request.Name;
        item.Quantity = request.Quantity;
        item.Unit = request.Unit;

        repository.Update(item);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Updated warehouse item {ItemId}", id);

        return item.ToDto();
    }

    public async Task DeleteItemAsync(Guid id, CancellationToken ct = default)
    {
        var item = await repository.GetByIdAsync(id, ct);
        if (item == null) throw new WarehouseItemNotFoundException(id);

        repository.Delete(item);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Deleted warehouse item {ItemId}", id);
    }

    public async Task DeductStockAsync(DeductStockRequest request, CancellationToken ct = default)
    {
        await deductValidator.ValidateAndThrowAsync(request, ct);

        var item = await repository.GetByIdAsync(request.ItemId, ct);
        if (item == null) throw new WarehouseItemNotFoundException(request.ItemId);

        if (item.Quantity < request.Quantity)
        {
            throw new InsufficientStockException(item.Name, request.Quantity, item.Quantity, item.Unit);
        }

        item.Quantity -= request.Quantity;
        repository.Update(item);
        await repository.SaveChangesAsync(ct);

        logger.LogInformation("Deducted {Quantity}{Unit} from warehouse item {ItemName}", request.Quantity, item.Unit, item.Name);
        await eventPublisher.PublishAsync(new StockDeductedEvent(item.Id, request.Quantity, item.Quantity), ct);
    }

    public async Task<RecipeCheckResponse> CheckRecipeAsync(RecipeCheckRequest request, CancellationToken ct = default)
    {
        var recipe = await recipeRepository.GetByProductIdAsync(request.ProductId, ct);
        if (recipe == null)
        {
            return new RecipeCheckResponse(true, "No recipe defined for this product, allowing order.");
        }

        var items = await repository.GetAllAsync(ct);

        foreach (var ingredient in recipe.Ingredients)
        {
            var needed = ingredient.QuantityRequired * request.Quantity;
            var item = items.FirstOrDefault(i =>
                string.Equals(i.Name, ingredient.IngredientName, StringComparison.OrdinalIgnoreCase));

            var have = item?.Quantity ?? 0m;
            if (item == null || have < needed)
            {
                return new RecipeCheckResponse(false,
                    $"Insufficient {ingredient.IngredientName}: need {needed}, have {have}.");
            }
        }

        return new RecipeCheckResponse(true);
    }

    public async Task<ConsumeRecipeResponse> ConsumeRecipeAsync(ConsumeRecipeRequest request, CancellationToken ct = default)
    {
        var recipe = await recipeRepository.GetByProductIdAsync(request.ProductId, ct);
        if (recipe == null)
        {
            return new ConsumeRecipeResponse(true, Message: "No recipe defined for this product, nothing to consume.");
        }

        var items = await repository.GetAllAsync(ct);

        // Two-pass: first verify everything is available, then deduct atomically.
        // Avoids partial deductions when a later ingredient is short.
        foreach (var ingredient in recipe.Ingredients)
        {
            var needed = ingredient.QuantityRequired * request.Quantity;
            var item = items.FirstOrDefault(i =>
                string.Equals(i.Name, ingredient.IngredientName, StringComparison.OrdinalIgnoreCase));

            var have = item?.Quantity ?? 0m;
            if (item == null || have < needed)
            {
                return new ConsumeRecipeResponse(
                    Consumed: false,
                    ShortageIngredientName: ingredient.IngredientName,
                    ShortageQuantityNeeded: needed,
                    ShortageQuantityAvailable: have,
                    ShortageUnit: ingredient.Unit,
                    Message: $"Insufficient {ingredient.IngredientName}: need {needed}{ingredient.Unit}, have {have}{ingredient.Unit}.");
            }
        }

        foreach (var ingredient in recipe.Ingredients)
        {
            var needed = ingredient.QuantityRequired * request.Quantity;
            var item = items.First(i =>
                string.Equals(i.Name, ingredient.IngredientName, StringComparison.OrdinalIgnoreCase));

            item.Quantity -= needed;
            repository.Update(item);
            await eventPublisher.PublishAsync(new StockDeductedEvent(item.Id, needed, item.Quantity), ct);
        }

        await repository.SaveChangesAsync(ct);
        logger.LogInformation(
            "Consumed recipe for product {ProductId} x{Quantity} ({IngredientCount} ingredients deducted)",
            request.ProductId, request.Quantity, recipe.Ingredients.Count);

        return new ConsumeRecipeResponse(true);
    }

    public async Task<List<RecipeDto>> GetAllRecipesAsync(CancellationToken ct = default)
    {
        var recipes = await recipeRepository.GetAllAsync(ct);
        return recipes.ToDtoList();
    }

    public async Task<RecipeDto> UpsertRecipeAsync(CreateRecipeRequest request, CancellationToken ct = default)
    {
        // Delete existing recipe for this product if it exists (full replace).
        await recipeRepository.DeleteByProductIdAsync(request.ProductId, ct);
        await recipeRepository.SaveChangesAsync(ct);

        var recipeId = Guid.NewGuid();
        var recipe = new Recipe
        {
            Id          = recipeId,
            ProductId   = request.ProductId,
            ProductName = request.ProductName,
            Ingredients = request.Ingredients.Select(i => new RecipeIngredient
            {
                Id               = Guid.NewGuid(),
                RecipeId         = recipeId,
                IngredientName   = i.IngredientName,
                QuantityRequired = i.QuantityRequired,
                Unit             = i.Unit
            }).ToList()
        };

        await recipeRepository.AddAsync(recipe, ct);
        await recipeRepository.SaveChangesAsync(ct);

        logger.LogInformation("Upserted recipe for product {ProductId} ({ProductName}, {Count} ingredients)",
            request.ProductId, request.ProductName, recipe.Ingredients.Count);

        return recipe.ToDto();
    }

    public async Task DeleteRecipeAsync(Guid productId, CancellationToken ct = default)
    {
        await recipeRepository.DeleteByProductIdAsync(productId, ct);
        await recipeRepository.SaveChangesAsync(ct);
        logger.LogInformation("Deleted recipe for product {ProductId}", productId);
    }
}
