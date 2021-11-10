# Umbraco.Forms.Integrations.Crm.Hubspot

This integration provides a custom workflow, allowing form entries to be mapped to a HubSpot contact record, and stored within the CRM platform.

## Prerequisites

Requires minimum versions of Umbraco:

- CMS: 8.1.0
- Forms: 8.7.4

## How To Use

### Authentication

The package supports two modes of authentication:

- API Key
- OAuth

#### API Key

Log into your HubSpot account, go to _Settings > Integrations > API Key_ and create an API key.

Add this to a setting in `UmbracoForms.config`:

```
<setting key="HubSpotApiKey" value="[your API key]" />
```

#### OAuth

The OAuth flow for authentication will be used if an API key is not configured.

When using the workflow, the user will be made aware that the installation is not authenticated with HubSpot and can't currently be used.

The will be prompted to click a link which will take them to the HubSpot authentication page for the Umbraco Forms HubSpot app.

They will need to log into their HubSpot account and agree to the permissions that the app requires (which is to be able to read and write contact information).

They will then be redirected to a website hosted at https://hubspot-forms-auth.umbraco.com which will present an authorization code issued by HubSpot. It's necessary to copy that and paste into the field indicated within the Umbraco Forms installation.

Behind the scenes this will make a further request to HubSpot which will return two tokens - an access token and a refresh token.  The former will be provided in further API calls to HubSpot to authenticate the request.  The latter will be stored via Umbraco's key/value service (which writes to the `umbracoKeyValue` table) and will be used to retrieve a new access token when it expires.

A button is available to clear the authentication with HubSpot, following which the authentication process would need to be repeated before the integration can again be used.

When the OAuth authentication method is being used, the API call to retrieve the token is proxied via and endpoint on the same website, allowing the Umbraco Forms HubSpot app secret key to remain secret (i.e. not included in configuration or hard-coded into the dll).

### Working With the HubSpot/Umbraco Forms Integration

Add the "Save Contact to Hubspot" workflow to a form and configure the mappings between the form and Hubspot fields.

![Select the HubSpot workflow](./img/select-workflow.png)

![Defining mappings](./img/mapping.png)

When a form is submitted on the website, the workflow will execute and create a new contact record in your Hubspot account, using the information mapped from the fields in the form submission.

![Hubspot contacts](./img/hubspot-contacts.png)

## For Further Consideration

- Currently the full set of contact properties are surfaced in the UI.  Further research needed to see if we can restrict this to only those used within the account, or to add custom properties.
- Validation in the Umbraco Forms form and the HubSpot contact record may not match (e.g. a field required by HubSpot may not be in the form).
