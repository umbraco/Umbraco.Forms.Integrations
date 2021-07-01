# Umbraco.Forms.Extensions.Crm.Hubspot
This adds a Hubspot extension to your Umbraco Forms installation

## How To Use

Log into your HubSpot account, go to _Settings > Integrations > API Key_ and create an API key.

Add this to a setting in `UmbracoForms.config`:

```
    <setting key="HubSpotApiKey" value="[your API key]" />
```

Add the "Save Contact to Hubspot" workflow to a form and configure the mappings between the form and Hubspot fields.

## Research
Hubspot allows you to add additional fields to a Contact but they seem you can only choose from a pre-determined list
This UI can also set required fields, so what should the workflow do if it does not provide the data required

![Hubspot UI](https://user-images.githubusercontent.com/1389894/122041936-7170aa00-cdd1-11eb-80ac-f9106c439599.png)

