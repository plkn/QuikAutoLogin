namespace AutoLoginTest.Config;

/// <summary>
/// Настройки одного экземпляра квика, который нужно автологинить.
/// </summary>
public record QuikConfiguration(string ExePath, string Login, string Password);