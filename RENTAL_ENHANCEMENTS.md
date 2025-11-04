# Rental Management Enhancement

This document describes the enhanced rental management features added to the InventoryManagementApp.

## Features Overview

### 1. Email Reminder System

Automatically sends email reminders to customers the day before their rental is due.

**Components:**
- `EmailService`: Handles sending emails via SMTP
- `RentalReminderService`: Scheduler that checks daily at 2:30 PM for rentals due tomorrow

**Configuration:**
All email settings are stored in the Settings database table and can be configured through the SettingsViewModel:
- SMTP Host
- SMTP Port
- SMTP Username & Password
- From Email Address
- From Name
- Enable SSL
- Contact Information (included in reminder emails)

**Usage:**
```csharp
// Create email service
var emailService = new EmailService(
    smtpHost: "smtp.gmail.com",
    smtpPort: 587,
    smtpUsername: "your@email.com",
    smtpPassword: "yourpassword",
    fromEmail: "rentals@company.com",
    fromName: "Equipment Rentals",
    enableSsl: true
);

// Create reminder service
var reminderService = new RentalReminderService(
    rentalService,
    emailService,
    contactInfo: "Call us at (555) 123-4567"
);

// Start the scheduler (checks daily at 2:30 PM)
reminderService.Start();
```

### 2. Rental Frequency Analytics

Track which items are rented most frequently to help with inventory planning.

**New Methods:**
- `RentalService.GetRentalFrequencyAsync(topN)`: Returns top N most rented items with counts
- `ReportService.GenerateRentalFrequencyReport(topN)`: Generates printable report

**Usage:**
```csharp
// Get top 20 most rented items
var frequencies = await rentalService.GetRentalFrequencyAsync(20);

foreach (var freq in frequencies)
{
    Console.WriteLine($"{freq.ItemNumber}: {freq.RentalCount} rentals");
}
```

**UI Integration:**
- Added "Most Rented Items" to the Reports menu in ReportsViewModel

### 3. Picking Slips and Invoices

Generate professional printable documents for rentals.

**Components:**
- `RentalPrintingService`: Generates FlowDocuments for printing

**Features:**

**Picking Slip:**
- Company information header
- Rental and customer details
- Item information and location
- Signature lines for picker and customer

**Invoice:**
- Professional invoice layout
- Itemized rental charges (calculated by days)
- Late fee support
- Company branding

**Usage:**
```csharp
var printService = new RentalPrintingService(
    companyName: "Equipment Rentals",
    companyAddress: "123 Main St",
    companyPhone: "(555) 123-4567"
);

// Generate picking slip
var pickingSlip = printService.GeneratePickingSlip(rental);

// Generate invoice with daily rate and late fee
var invoice = printService.GenerateInvoice(rental, dailyRate: 25.00m, lateFee: 10.00m);
```

**UI Integration:**
- Added `PrintPickingSlipCommand` to ManageRentalsViewModel
- Added `PrintInvoiceCommand` to ManageRentalsViewModel

### 4. Rental Configuration Service

Centralized configuration management for rental-related settings.

**RentalConfigurationService Methods:**
- Default daily rental rate
- Default late fee amount
- Email enabled/disabled
- Email SMTP settings
- Company information

**Usage:**
```csharp
var configService = new RentalConfigurationService(settingsService);

// Get/set daily rate
var dailyRate = await configService.GetDefaultDailyRateAsync();
await configService.SetDefaultDailyRateAsync(30.00m);

// Get/set late fee
var lateFee = await configService.GetDefaultLateFeeAsync();
await configService.SetDefaultLateFeeAsync(15.00m);
```

## Configuration

### appsettings.json

Default configuration values can be set in appsettings.json:

```json
{
  "Email": {
    "SmtpHost": "smtp.example.com",
    "SmtpPort": 587,
    "SmtpUsername": "",
    "SmtpPassword": "",
    "FromEmail": "rentals@example.com",
    "FromName": "Equipment Rentals",
    "EnableSsl": true,
    "ContactInfo": "Contact us at rentals@example.com or call (555) 123-4567"
  },
  "Company": {
    "Name": "Equipment Rentals",
    "Address": "123 Main Street, City, State 12345",
    "Phone": "(555) 123-4567"
  }
}
```

### Database Settings

All configuration is also stored in the Settings table for runtime modification:
- `Rental.DefaultDailyRate`: Default rental rate per day
- `Rental.DefaultLateFee`: Default late fee amount
- `Rental.ReminderEnabled`: Enable/disable reminder emails
- `Email.Enabled`: Enable/disable email functionality
- `Email.*`: SMTP configuration
- `Company.*`: Company information for documents

## Integration

### Application Startup

To integrate the rental reminder service in your application:

```csharp
// In App.xaml.cs or your startup code:

// Create services
var emailService = new EmailService(...);
var reminderService = new RentalReminderService(rentalService, emailService, contactInfo);

// Start the reminder scheduler
reminderService.Start();

// Clean up on exit
Application.Current.Exit += (s, e) => 
{
    reminderService.Stop();
    emailService.Dispose();
};
```

### ViewModel Commands

The ManageRentalsViewModel now includes:
- `PrintPickingSlipCommand`: Print a picking slip for the selected rental
- `PrintInvoiceCommand`: Print an invoice for the selected rental

These commands can be bound to UI buttons:

```xaml
<Button Content="Print Picking Slip" 
        Command="{Binding PrintPickingSlipCommand}" />
<Button Content="Print Invoice" 
        Command="{Binding PrintInvoiceCommand}" />
```

## Testing

Unit tests are included for all new services:

- `EmailServiceTests`: Tests email service functionality
- `RentalPrintingServiceTests`: Tests document generation
- `RentalFrequencyTests`: Tests rental analytics
- `RentalReminderServiceTests`: Tests reminder scheduler

Run tests with:
```bash
dotnet test
```

## Security Considerations

1. **SMTP Credentials**: Store SMTP passwords securely. Consider using encrypted storage or environment variables in production.

2. **Email Validation**: Always validate customer email addresses before sending.

3. **Rate Limiting**: Consider implementing rate limiting for email sending to avoid spam filters.

4. **Settings Access**: Rental configuration settings require admin privileges to modify (enforced by ISettingsService).

## Troubleshooting

### Emails Not Sending

1. Check SMTP configuration in Settings
2. Verify SMTP credentials are correct
3. Check if `Email.Enabled` setting is true
4. Check firewall/network access to SMTP server
5. Review application logs for detailed error messages

### Reminders Not Sent at Correct Time

1. Verify system clock is correct
2. Check that ReminderService.Start() was called
3. Review logs for scheduler execution
4. Ensure email service is properly configured

### Picking Slips/Invoices Missing Information

1. Ensure rental has all required customer information
2. Check company settings in appsettings.json or Settings table
3. Verify rental dates are set correctly

## Future Enhancements

Potential future improvements:
- Multiple email templates for different rental types
- SMS reminder support
- Bulk invoice generation
- Rental revenue reporting
- Custom invoice branding with logo upload
- Email delivery status tracking
