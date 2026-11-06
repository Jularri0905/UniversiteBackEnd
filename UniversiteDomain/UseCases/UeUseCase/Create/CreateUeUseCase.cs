using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.UeUseCases.Create;

public class CreateUeUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Ue> ExecuteAsync(string numeroUe, string nomUe)
    {
        var ue = new Ue { NumeroUe = numeroUe, Intitule = nomUe };
        return await ExecuteAsync(ue);
    }

    public async Task<Ue> ExecuteAsync(Ue ue)
    {
        await CheckBusinessRules(ue);
        Ue ueCree = await repositoryFactory.UeRepository().CreateAsync(ue);
        repositoryFactory.SaveChangesAsync().Wait();
        return ueCree;
    }

    private async Task CheckBusinessRules(Ue ue)
    {
        ArgumentNullException.ThrowIfNull(ue);
        ArgumentNullException.ThrowIfNull(ue.NumeroUe);
        ArgumentNullException.ThrowIfNull(ue.Intitule);
        ArgumentNullException.ThrowIfNull(repositoryFactory);

        // 1. Règle métier : l'intitulé doit contenir plus de 3 caractères (donc > 3, soit au moins 4)
        if (ue.Intitule.Length <= 3) 
            throw new InvalidNomUeException($"{ue.Intitule} est incorrect - L'intitulé d'une UE doit contenir plus de 3 caractères");

        // 2. Règle métier : deux UEs ne peuvent pas porter le même numéro
        var existe = await repositoryFactory.UeRepository().FindByConditionAsync(u => u.NumeroUe.Equals(ue.NumeroUe));

        if (existe is { Count: > 0 }) 
            throw new DuplicateNumeroUeException($"{ue.NumeroUe} - ce numéro d'UE est déjà affecté à une autre UE");
    }
}