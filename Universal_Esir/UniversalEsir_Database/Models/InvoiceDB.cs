using System;
using System.Collections.Generic;

namespace UniversalEsir_Database.Models
{
    public partial class InvoiceDB
    {
        public InvoiceDB()
        {
            ItemInvoices = new HashSet<ItemInvoiceDB>();
            Orders = new HashSet<OrderDB>();
            PaymentInvoices = new HashSet<PaymentInvoiceDB>();
            TaxItemInvoices = new HashSet<TaxItemInvoiceDB>();
            DriverInvoices = new HashSet<DriverInvoiceDB>();
        }

        public string Id { get; set; } = null!;
        public int ClanId { get; set; }
        public string? Porudzbenica { get; set; }
        public string? KnjizenjePazaraId { get; set; }
        public DateTime? DateAndTimeOfIssue { get; set; }
        public string? Cashier { get; set; }
        public string? BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerAddress { get; set; }
        public string? BuyerCostCenterId { get; set; }
        public int? InvoiceType { get; set; }
        public int? TransactionType { get; set; }
        public string? ReferentDocumentNumber { get; set; }
        public DateTime? ReferentDocumentDt { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? RequestedBy { get; set; }
        public string? InvoiceNumberResult { get; set; }
        public DateTime? SdcDateTime { get; set; }
        public string? InvoiceCounter { get; set; }
        public string? InvoiceCounterExtension { get; set; }
        public string? SignedBy { get; set; }
        public string? EncryptedInternalData { get; set; }
        public string? Signature { get; set; }
        public int? TotalCounter { get; set; }
        public int? TransactionTypeCounter { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? TaxGroupRevision { get; set; }
        public string? BusinessName { get; set; }
        public string? Tin { get; set; }
        public string? LocationName { get; set; }
        public string? Address { get; set; }
        public string? District { get; set; }
        public string? Mrc { get; set; }
        public int IsSend { get; set; }

        public virtual KnjizenjePazaraDB? KnjizenjePazara { get; set; }
        public virtual ICollection<ItemInvoiceDB> ItemInvoices { get; set; }
        public virtual ICollection<OrderDB> Orders { get; set; }
        public virtual ICollection<PaymentInvoiceDB> PaymentInvoices { get; set; }
        public virtual ICollection<TaxItemInvoiceDB> TaxItemInvoices { get; set; }
        public virtual ICollection<DriverInvoiceDB> DriverInvoices { get; set; }
    }
}
