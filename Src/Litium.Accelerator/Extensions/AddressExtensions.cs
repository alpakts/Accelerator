using Litium.Sales;

namespace Litium.Accelerator.Extensions
{
    public static class AddressExtensions
    {
        /// <summary>
        /// Check the address object is not null or empty.
        /// </summary>
        /// <param name="address">Address object.</param>
        /// <returns>If Address is empty, return true. Else, return false.</returns>
        public static bool IsEmpty(this Address address)
        {
            if (address == null)
                return true;

            return string.IsNullOrEmpty(address.Address1) && string.IsNullOrEmpty(address.CareOf) && string.IsNullOrEmpty(address.City)
                && string.IsNullOrEmpty(address.Country) && string.IsNullOrEmpty(address.ZipCode) && string.IsNullOrEmpty(address.Email)
                && string.IsNullOrEmpty(address.PhoneNumber) && string.IsNullOrEmpty(address.FirstName) && string.IsNullOrEmpty(address.LastName);
        }
    }
}
