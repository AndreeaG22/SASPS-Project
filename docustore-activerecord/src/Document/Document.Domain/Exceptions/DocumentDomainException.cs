namespace Document.Domain.Exceptions;

public class DocumentDomainException : Exception
{
    public DocumentDomainException(string message) : base(message)
    {
    }

    public DocumentDomainException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}