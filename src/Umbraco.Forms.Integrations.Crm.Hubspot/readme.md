# Umbraco.Forms.Integrations.Crm.Hubspot

This integration provides a custom workflow, allowing form entries to be mapped to a HubSpot contact record, and stored within the CRM platform.

## Prerequisites

Requires minimum versions of Umbraco:

- CMS: 8.1.0
- Forms: 8.7.4

## How To Use

Log into your HubSpot account, go to _Settings > Integrations > API Key_ and create an API key.

Add this to a setting in `UmbracoForms.config`:

```
<setting key="HubSpotApiKey" value="[your API key]" />
```

Add the "Save Contact to Hubspot" workflow to a form and configure the mappings between the form and Hubspot fields.

![Select the HubSpot workflow](./img/select-workflow.png)

![Defining mappings](./img/mapping.png)

When a form is submitted on the website, the workflow will execute and create a new contact record in your Hubspot account, using the information mapped from the fields in the form submission.

![Hubspot contacts](./img/hubspot-contacts.png)

## For Further Consideration

- Currently the full set of contact properties are surfaced in the UI.  Further research needed to see if we can restrict this to only those used within the account, or to add custom properties.
- Validation in the Umbraco Forms form and the HubSpot contact record may not match (e.g. a field required by HubSpot may not be in the form).
