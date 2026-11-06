namespace UniversiteDomain.Entities;
using System.Collections.Generic;

public class Parcours
{
    public long Id { get; set; }
    public string NomParcours { get; set; } = string.Empty;
    public string AnneeFormation { get; set; } = string.Empty;
    
    // OneToMany : un parcours contient plusieurs étudiants
    // Remarque : pour éviter quelques NullPointerException disgracieux, j'ai choisi de créer une liste d'incrits vide quand aucun étudiant n'est inscrit dans un parcours plutôt que de l'initialiser à null
    public List<Etudiant>? Inscrits { get; set; } = new();

    public override string ToString()
    {
        return "ID "+Id +" : "+NomParcours+" - Master "+AnneeFormation;
    }
}