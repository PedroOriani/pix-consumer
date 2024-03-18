namespace PixConsumer.DTOs
{
    public class PaymentDTO
    {
        public int Id { get; set; }
        public OriginDto Origin { get; set; }
        public DestinyDto Destiny { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
    }

    public class OriginDto
    {
        public UserDto User { get; set; }
        public AccountDto Account { get; set; }
    }

    public class UserDto
    {
        public string Cpf { get; set; }
    }

    public class AccountDto
    {
        public string Number { get; set; }
        public string Agency { get; set; }
    }

    public class DestinyDto
    {
        public KeyDto Key { get; set; }
    }

    public class KeyDto
    {
        public string Value { get; set; }
        public string Type { get; set; }
    }
}
