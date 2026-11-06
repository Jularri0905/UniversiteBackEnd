using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.ParcoursExceptions;

namespace UniversiteDomain.UseCases.ParcoursUseCases.Create;

public class CreateParcoursUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Parcours> ExecuteAsync(string nomParcours, string anneeFormation)
    {
        var parcours = new Parcours { NomParcours = nomParcours, AnneeFormation = anneeFormation };
        return await ExecuteAsync(parcours);
    }

    public async Task<Parcours> ExecuteAsync(Parcours parcours)
    {
        await CheckBusinessRules(parcours);
        Parcours p = await repositoryFactory.ParcoursRepository().CreateAsync(parcours);
        await repositoryFactory.SaveChangesAsync();
        return p;
    }

    private async Task CheckBusinessRules(Parcours parcours)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(parcours.NomParcours);
        ArgumentNullException.ThrowIfNull(repositoryFactory);

        // Nom du parcours doit contenir plus de 3 caractères
        if (parcours.NomParcours.Length < 3) 
            throw new InvalidNomParcoursException(parcours.NomParcours + " incorrect - Le nom d'un parcours doit contenir plus de 3 caractères");

        // L'année de formation doit être exclusivement "M1" ou "M2"
        if (!parcours.AnneeFormation.Equals("M1", StringComparison.OrdinalIgnoreCase) && 
            !parcours.AnneeFormation.Equals("M2", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidAnneeFormationException(parcours.AnneeFormation + " incorrect - L'année de formation doit être 'M1' ou 'M2'");
        }

        // Vérification des doublons (un parcours avec le même nom et la même année)
        List<Parcours> existe = await repositoryFactory.ParcoursRepository().FindByConditionAsync(p => 
            p.NomParcours.Equals(parcours.NomParcours) && p.AnneeFormation == parcours.AnneeFormation);

        if (existe is { Count: > 0 }) 
            throw new DuplicateParcoursException(parcours.NomParcours + " (Année " + parcours.AnneeFormation + ") existe déjà");
    }
}