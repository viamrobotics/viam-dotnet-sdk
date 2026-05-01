namespace Viam.ModularResources
{
    public class ValidationException(string param, string message) : Exception($"{param} failed validation: {message}");
}