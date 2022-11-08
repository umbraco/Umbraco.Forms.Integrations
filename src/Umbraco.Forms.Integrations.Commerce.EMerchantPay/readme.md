# Umbraco.Forms.Integrations.Commerce.emerchantpay

This integration provides a custom workflow for handling online payments using a hosted payment page provided by emerchantpay.

## Prerequisites

Required minimum versions of Umbraco CMS: 
- CMS: 8.5.4
- Forms: 8.13.0

## How To Use

### Authentication

All requests to emerchantpay API are authenticated by providing the merchant's username and password.

If the configuration is incomplete, the user will receive an error message.

### Configuration

The emerchantpay API endpoints accept and return XML data. When the form is submitted, two batches of payload data are exchanged: one for handling consumer information and the second one for creating the payment.

A consumer has the following available properties that can be mapped against form fields:
* Email
* FirstName
* LastName
* Address1
* Address2
* ZipCode
* City
* State
* Country
* Phone

The below configuration - consisting of authentication settings, merchant specific details and customizable payment fields - is required. Some configuration items
are stored as an array of strings or a dictionary, and parsed using a specific service.

```
<appSettings>
...
<add key="Umbraco.Forms.Integrations.Commerce.eMerchantPay.GatewayBaseurl" value="https://staging.gate.emerchantpay.net/"/>
<add key="Umbraco.Forms.Integrations.Commerce.eMerchantPay.WpfUrl" value="https://staging.wpf.emerchantpay.net/wpf"/>
<add key="Umbraco.Forms.Integrations.Commerce.eMerchantPay.Username" value="[your_merchant_username]"/>
<add key="Umbraco.Forms.Integrations.Commerce.eMerchantPay.Password" value="[your_merchant_password]"/>
<add key="Umbraco.Forms.Integrations.Commerce.eMerchantPay.UmbracoBaseUrl" value="[your_website_url]"/>
<add key="Umbraco.Forms.Integrations.Commerce.eMerchantPay.Supplier" value="Umbraco"/>
<add key="Umbraco.Forms.Integrations.Commerce.eMerchantPay.Usage" value="Payment Gateway using Umbraco Forms"/>
<add key="Umbraco.Forms.Integrations.Commerce.eMerchantPay.Currencies" value="USD,US Dollar;EUR,Euro;GBP,British Pound;DKK,Danish Krone"/>
<add key="Umbraco.Forms.Integrations.Commerce.eMerchantPay.TransactionTypes" value="authorize;sale"/>
<add key="Umbraco.Forms.Integrations.Commerce.eMerchantPay.MappingFields" value="Email;FirstName;LastName"/>
...
</appSettings>
```

### Working with the Umbraco Forms - emerchantpay integration

To use it you will need to attach the _emerchantpay Gateway_ to a form and map the _Amount_, _Currency_, _Number of Items_, _Record Status_, _Record Payment Unique ID_ and _Consumer Details_ with matching form fields, then configure the event handlers
for payment sucessfully processed, failed or cancelled.

When a form is submitted on the website, the workflow will execute and based on it's settings, two data payloads will be sent to emerchantpay for creating or retrieving the details of a consumer, and for creating a payment.
The response for the second request will provide the URL for the hosted payment page, and the user will the redirected there. 

On completing the payment the emerchantpay API will return the user to the page provided in matching event handler of the worklow.
