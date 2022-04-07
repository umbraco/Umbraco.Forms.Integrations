# Umbraco.Forms.Integrations.Automation.Zapier

This integration provides a custom workflow allowing form entries to be mapped to the fields of a Zap tool trigger from Zapier.

A Zap is an automated workflow that connects various apps and services together. Each Zap consists of a trigger and one or more actions.

## Prerequisites

Requires minimum versions of Umbraco:

- CMS: 8.1.0
- Forms: 8.9.1

## How To Use

### Authentication

For this integration, the authentication is managed on Zapier's side by using the Umbraco marketplace app. 

The Umbraco app manages two types of events:
* New Form Submission - triggers when a form is submitted
* New Content Published - triggers when a new content has been published.

The trigger event to be used by this integration is _New Form Submission_.

When creating the Zap trigger, you will be prompted to enter a username, password and the URL for your Umbraco website.

Then the Umbraco application will validate the credentials entered and return a message in case the validation fails.

If you want to extend the security layer, you can also specify a user group that the user trying to connect needs to be a part of, by adding the following 
setting in `Web.config`:

```
<appSettings>
...
  <add key="Umbraco.Cms.Integrations.Automation.Zapier.UserGroup" value="[your User Group]" />
...
</appSettings>
```

### Working With the Zapier Forms Integration
Add the _Trigger Zap_ workflow to a form, configure which fields will be sent to the Zap and add the webhook URL of the Zap.

You can add mappings for each of the form's fields (if no mapping is created, all fields will be included by default), and also pick one of the
standard fields: Form ID, Form Name, Page URL, Submission date/time.

When the form is submitted, the workflow will be invoked and a POST request, with the content built based on the workflow's settings, will trigger the Zap and the actions will be executed.
