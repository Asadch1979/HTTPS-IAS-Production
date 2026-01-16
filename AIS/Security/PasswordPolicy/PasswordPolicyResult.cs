namespace AIS.Security.PasswordPolicy
{
    public class PasswordPolicyResult
    {
        public PasswordPolicyResult(bool isValid, string errorMessage)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public bool IsValid { get; }

        public string ErrorMessage { get; }
    }
}
