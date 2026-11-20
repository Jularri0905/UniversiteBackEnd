namespace UniversiteDomain.Exceptions.NoteExceptions;
using System;

[Serializable]
public class DuplicateNoteException : Exception
{
    public DuplicateNoteException() : base() { }
    public DuplicateNoteException(string message) : base(message) { }
    public DuplicateNoteException(string message, Exception inner) : base(message, inner) { }
}