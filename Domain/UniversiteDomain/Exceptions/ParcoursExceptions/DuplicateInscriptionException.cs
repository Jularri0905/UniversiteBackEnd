namespace UniversiteDomain.Exceptions.ParcoursExceptions;
using System;

[Serializable]
public class DuplicateInscriptionException : Exception
{
    public DuplicateInscriptionException() : base() { }
    public DuplicateInscriptionException(string message) : base(message) { }
    public DuplicateInscriptionException(string message, Exception inner) : base(message, inner) { }
}