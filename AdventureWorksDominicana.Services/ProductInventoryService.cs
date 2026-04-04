using AdventureWorksDominicana.Data.Context;
using AdventureWorksDominicana.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AdventureWorksDominicana.Services;

public class ProductInventoryService(IDbContextFactory<Contexto> dbContextFactory)
{
    public async Task<ProductInventory?> Buscar(int productId, short locationId)
    {
        await using var contexto = await dbContextFactory.CreateDbContextAsync();

        return await contexto.ProductInventories.AsNoTracking().Include(pi => pi.Product).Include(pi => pi.Location).FirstOrDefaultAsync(pi => pi.ProductId == productId && pi.LocationId == locationId);
    }

    public async Task<List<ProductInventory>> GetList(Expression<Func<ProductInventory, bool>> criterio)
    {
        await using var contexto = await dbContextFactory.CreateDbContextAsync();

        return await contexto.ProductInventories.AsNoTracking().Include(pi => pi.Product).Include(pi => pi.Location).Where(criterio).ToListAsync();
    }

    public async Task<bool> Existe(int productId, short locationId)
    {
        await using var contexto = await dbContextFactory.CreateDbContextAsync();
        return await contexto.ProductInventories.AnyAsync(pi => pi.ProductId == productId && pi.LocationId == locationId);
    }

    public async Task<bool> Guardar(ProductInventory productInventory)
    {
        if (!await Existe(productInventory.ProductId, productInventory.LocationId))
        {
            return await Insertar(productInventory);
        }
        else
        {
            return await Modificar(productInventory);

        }
    }

    private async Task<bool> Insertar(ProductInventory productInventory)
    {
        await using var contexto = await dbContextFactory.CreateDbContextAsync();
        productInventory.Rowguid = Guid.NewGuid();
        productInventory.ModifiedDate = DateTime.Now;

        productInventory.Product = null!;
        productInventory.Location = null!;

        contexto.ProductInventories.Add(productInventory);
        return await contexto.SaveChangesAsync() > 0;
    }

    private async Task<bool> Modificar(ProductInventory productInventory)
    {
        await using var contexto = await dbContextFactory.CreateDbContextAsync();

        productInventory.ModifiedDate = DateTime.Now;

        productInventory.Product = null!;
        productInventory.Location = null!;

        contexto.ProductInventories.Update(productInventory);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> Eliminar(int productId, short locationId)
    {
        await using var contexto = await dbContextFactory.CreateDbContextAsync();

        try
        {
            var filas = await contexto.ProductInventories.Where(pi => pi.ProductId == productId && pi.LocationId == locationId).ExecuteDeleteAsync();
            return filas > 0;
        }
        catch (DbUpdateException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }
}