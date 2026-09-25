using System.Security.Claims;

namespace TimewareServiceHub.Api.Extensions;

// "Extension method": adiciona um método a um tipo que não é seu (ClaimsPrincipal é do framework).
// Não existe equivalente direto em Java: lá você criaria uma classe utilitária estática (SecurityUtils.getUserId(auth)).
// Em C# você escreve User.ObterUsuarioId() como se o método fosse da própria classe.
public static class ClaimsPrincipalExtensions
{
    public static int ObterUsuarioId(this ClaimsPrincipal user)
    {
        // "sub" (subject) é a claim padrão do JWT com o identificador do usuário.
        var valor = user.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Token sem identificação do usuário.");
        return int.Parse(valor);
    }
}
