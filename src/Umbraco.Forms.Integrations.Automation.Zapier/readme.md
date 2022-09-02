# Umbraco.Forms.Integrations.Automation.Zapier

This integration is an add-on to the Umbraco CMS integration available [here](https://github.com/umbraco/Umbraco.Cms.Integrations/tree/main/src/Umbraco.Cms.Integrations.Automation.Zapier), and provides necessary components for handling form submissions based on the registered subscription hooks.

A Zap is an automated workflow that connects various apps and services together. Each Zap consists of a trigger and one or more actions.

## Prerequisites

Requires minimum versions of Umbraco:

- CMS: 10.1.0
- Forms: 10.1.0

## How To Use

### Authentication

For this integration, the authentication is managed on Zapier's side by using the Umbraco marketplace app. 

The Umbraco app manages two types of events:
* New Form Submission - triggers when a form is submitted
* New Content Published - triggers when a new content has been published.

The trigger event to be used by this integration is _New Form Submission_.

When creating the Zap trigger, you will be prompted to enter a username, password and the URL for your Umbraco website, or you can use instead an API key.
If the following setting is present, then the API key based authentication will take precendence and will be the main method of authorization.
```
"Umbraco": {
...
  "Forms": {
    "Integrations": {
      "Automation": {
        "Zapier": {
          "Settings": {
            "ApiKey": "[your_api_key]"
          }
        }
      }
    }
  }
...
}
```

If no API key is present, then the Umbraco application will validate the credentials entered and return a message in case the validation fails.

If you want to extend the security layer, you can also specify a user group that the user trying to connect needs to be a part of, by adding the following 
setting in `appsettings.json`:

```
"Umbraco": {
...
  "Settings": {
    "UserGroup": "[your_user_group]"
  }
...
}
```

### Working With the Zapier Forms Integration
In the _Content_ area of the backoffice, find the _Zapier Integrations_ dashboard.

The dashboard is composed of two sections:
* Content Properties - Zapier details and input fields for adding content configurations
* Registered Subscription Hooks - list of registered entities.

Subscription hooks are split in two categories: 
* 1 = Content
* 2 = Form

The _Trigger Webhook_ action will send a test request to the Zap trigger, enabling the preview of requests in the Zap setup workflow.
