using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Services;
using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Integrations.Crm.Hubspot.Configuration;
using Umbraco.Forms.Integrations.Crm.Hubspot.Models;
using Umbraco.Forms.Integrations.Crm.Hubspot.Services;

namespace Umbraco.Forms.Integrations.Crm.Hubspot.Tests;

public class HubspotContactServiceTests
{
    private const string ApiKey = "test-api-key";
    private readonly string s_contactPropertiesResponse = @"{
   ""results"":[
      {
         ""updatedAt"":""2019-10-14T20:45:41.715Z"",
         ""createdAt"":""2019-08-06T02:41:08.029Z"",
         ""name"":""firstname"",
         ""label"":""First Name"",
         ""type"":""string"",
         ""fieldType"":""text"",
         ""description"":""A contact's first name"",
         ""groupName"":""contactinformation"",
         ""options"":[
         ],
         ""displayOrder"":0,
         ""calculated"":false,
         ""externalOptions"":false,
         ""hasUniqueValue"":false,
         ""hidden"":false,
         ""hubspotDefined"":true,
         ""modificationMetadata"":{
            ""archivable"":true,
            ""readOnlyDefinition"":true,
            ""readOnlyValue"":false
         },
         ""formField"":true
      },
      {
         ""updatedAt"":""2019-10-14T20:45:41.796Z"",
         ""createdAt"":""2019-08-06T02:41:08.109Z"",
         ""name"":""lastname"",
         ""label"":""Last Name"",
         ""type"":""string"",
         ""fieldType"":""text"",
         ""description"":""A contact's last name"",
         ""groupName"":""contactinformation"",
         ""options"":[
         ],
         ""displayOrder"":1,
         ""calculated"":false,
         ""externalOptions"":false,
         ""hasUniqueValue"":false,
         ""hidden"":false,
         ""hubspotDefined"":true,
         ""modificationMetadata"":{
            ""archivable"":true,
            ""readOnlyDefinition"":true,
            ""readOnlyValue"":false
         },
         ""formField"":true
      },
      {
    ""updatedAt"":""2019-10-14T20:45:42.027Z"",
         ""createdAt"":""2019-08-06T02:41:08.204Z"",
         ""name"":""email"",
         ""label"":""Email"",
         ""type"":""string"",
         ""fieldType"":""text"",
         ""description"":""A contact's email address"",
         ""groupName"":""contactinformation"",
         ""options"":[
         ],
         ""displayOrder"":3,
         ""calculated"":false,
         ""externalOptions"":false,
         ""hasUniqueValue"":false,
         ""hidden"":false,
         ""hubspotDefined"":true,
         ""modificationMetadata"":{
        ""archivable"":true,
            ""readOnlyDefinition"":true,
            ""readOnlyValue"":false
         },
         ""formField"":true
      }
   ]
}";

    [Test]
    public async Task GetContactProperties_WithoutApiKeyConfigured_ReturnsEmptyCollectionWithLoggedWarning()
    {
        Mock<IOptions<HubspotSettings>> mockedConfig = CreateMockedConfiguration(withApiKey: false);
        var mockedHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockedLogger = new Mock<ILogger<HubspotContactService>>();
        var mockedKeyValueService = new Mock<IKeyValueService>();
        var sut = new HubspotContactService(mockedHttpClientFactory.Object, mockedConfig.Object, mockedLogger.Object, AppCaches.NoCache, mockedKeyValueService.Object);

        var result = await sut.GetContactPropertiesAsync();

        mockedLogger.Verify(
            m => m.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            It.IsAny<string>());

        Assert.IsEmpty(result);
    }

    [Test]
    public async Task GetContactProperties_WithFailedRequest_ReturnsEmptyCollectionWithLoggedError()
    {
        Mock<IOptions<HubspotSettings>> mockedConfig = CreateMockedConfiguration();

        var mockedHttpClientFactory = new Mock<IHttpClientFactory>();
        mockedHttpClientFactory
            .Setup<HttpClient>(x => x.CreateClient(It.IsAny<string>()))
            .Returns(CreateMockedHttpClient(HttpStatusCode.InternalServerError));
        
        var mockedLogger = new Mock<ILogger<HubspotContactService>>();
        var mockedKeyValueService = new Mock<IKeyValueService>();
        var sut = new HubspotContactService(mockedHttpClientFactory.Object, mockedConfig.Object, mockedLogger.Object, AppCaches.NoCache, mockedKeyValueService.Object);

        var result = await sut.GetContactPropertiesAsync();

        mockedLogger.Verify(
            m => m.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            It.IsAny<string>());

        Assert.IsEmpty(result);
    }

    [Test]
    public async Task GetContactProperties_WithSuccessfulRequest_ReturnsMappedAndOrderedPropertyCollection()
    {
        Mock<IOptions<HubspotSettings>> mockedConfig = CreateMockedConfiguration();

        var mockedHttpClientFactory = new Mock<IHttpClientFactory>();
        mockedHttpClientFactory
            .Setup<HttpClient>(x => x.CreateClient(It.IsAny<string>()))
            .Returns(CreateMockedHttpClient(HttpStatusCode.OK, s_contactPropertiesResponse));

        var mockedLogger = new Mock<ILogger<HubspotContactService>>();
        var mockedKeyValueService = new Mock<IKeyValueService>();
        var sut = new HubspotContactService(mockedHttpClientFactory.Object, mockedConfig.Object, mockedLogger.Object, AppCaches.NoCache, mockedKeyValueService.Object);

        var result = await sut.GetContactPropertiesAsync();

        Assert.AreEqual(3, result.Count());
        Assert.AreEqual("Email,First Name,Last Name", string.Join(",", result.Select(x => x.Label)));
        Assert.AreEqual("email,firstname,lastname", string.Join(",", result.Select(x => x.Name)));
    }

    [Test]
    public async Task PostContact_WithoutApiKeyConfigured_ReturnsNotConfiguredWithLoggedWarning()
    {
        Mock<IOptions<HubspotSettings>> mockedConfig = CreateMockedConfiguration(withApiKey: false);
        var mockedHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockedLogger = new Mock<ILogger<HubspotContactService>>();
        var mockedKeyValueService = new Mock<IKeyValueService>();
        var sut = new HubspotContactService( mockedHttpClientFactory.Object, mockedConfig.Object, mockedLogger.Object, AppCaches.NoCache, mockedKeyValueService.Object);

        var record = new Record();
        var fieldMappings = new List<MappedProperty>();
        var result = await sut.PostContactAsync(record, fieldMappings);

        mockedLogger.Verify(
            m => m.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            It.IsAny<string>());

        Assert.AreEqual(CommandResult.NotConfigured, result);
    }

    [Test]
    public async Task PostContact_WithFailedRequest_ReturnsFailedWithLoggedError()
    {
        Mock<IOptions<HubspotSettings>> mockedConfig = CreateMockedConfiguration();

        var mockedHttpClientFactory = new Mock<IHttpClientFactory>();
        mockedHttpClientFactory
            .Setup<HttpClient>(x => x.CreateClient(It.IsAny<string>()))
            .Returns(CreateMockedHttpClient(HttpStatusCode.InternalServerError));

        var mockedLogger = new Mock<ILogger<HubspotContactService>>();
        var mockedKeyValueService = new Mock<IKeyValueService>();
        var sut = new HubspotContactService(mockedHttpClientFactory.Object, mockedConfig.Object, mockedLogger.Object, AppCaches.NoCache, mockedKeyValueService.Object);

        var record = new Record();
        var fieldMappings = new List<MappedProperty>();
        var result = await sut.PostContactAsync(record, fieldMappings);

        mockedLogger.Verify(
        m => m.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once,
        It.IsAny<string>());

        Assert.AreEqual(CommandResult.Failed, result);
    }

    [Test]
    public async Task PostContact_WithSuccessfulRequest_ReturnSuccess()
    {
        Mock<IOptions<HubspotSettings>> mockedConfig = CreateMockedConfiguration();

        var mockedHttpClientFactory = new Mock<IHttpClientFactory>();
        mockedHttpClientFactory
            .Setup<HttpClient>(x => x.CreateClient(It.IsAny<string>()))
            .Returns(CreateMockedHttpClient(HttpStatusCode.OK));

        var mockedLogger = new Mock<ILogger<HubspotContactService>>();
        var mockedKeyValueService = new Mock<IKeyValueService>();
        var sut = new HubspotContactService(mockedHttpClientFactory.Object, mockedConfig.Object, mockedLogger.Object, AppCaches.NoCache, mockedKeyValueService.Object);

        var formFieldId = Guid.NewGuid();
        var record = new Record();
        record.RecordFields.Add(formFieldId, new RecordField
        {
            FieldId = formFieldId,
            Values = new List<object> { "Fred" }
        });
        var fieldMappings = new List<MappedProperty>()
        {
            new MappedProperty
            {
                FormField = formFieldId.ToString(),
                HubspotField = "firstname"
            }
        };
        var result = await sut.PostContactAsync(record, fieldMappings);

        mockedLogger.Verify(
           m => m.Log(
               LogLevel.Warning,
               It.IsAny<EventId>(),
               It.IsAny<It.IsAnyType>(),
               It.IsAny<Exception>(),
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
           Times.Never,
           It.IsAny<string>());

        Assert.AreEqual(CommandResult.Success, result);
    }

    [Test]
    public async Task PostContact_WithSuccessfulRequestAndUnmappedField_ReturnSuccessWithLoggedWarning()
    {
        Mock<IOptions<HubspotSettings>> mockedConfig = CreateMockedConfiguration();

        var mockedHttpClientFactory = new Mock<IHttpClientFactory>();
        mockedHttpClientFactory
          .Setup<HttpClient>(x => x.CreateClient(It.IsAny<string>()))
          .Returns(CreateMockedHttpClient(HttpStatusCode.OK));

        var mockedLogger = new Mock<ILogger<HubspotContactService>>();
        var mockedKeyValueService = new Mock<IKeyValueService>();
        var sut = new HubspotContactService(mockedHttpClientFactory.Object, mockedConfig.Object, mockedLogger.Object, AppCaches.NoCache, mockedKeyValueService.Object);

        var formFieldId = Guid.NewGuid();
        var record = new Record();
        var fieldMappings = new List<MappedProperty>()
        {
            new MappedProperty
            {
                FormField = formFieldId.ToString(),
                HubspotField = "firstname"
            }
        };
        var result = await sut.PostContactAsync(record, fieldMappings);

        mockedLogger.Verify(
           m => m.Log(
               LogLevel.Warning,
               It.IsAny<EventId>(),
               It.IsAny<It.IsAnyType>(),
               It.IsAny<Exception>(),
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
           Times.Once,
           It.IsAny<string>());

        Assert.AreEqual(CommandResult.Success, result);
    }

    private static Mock<IOptions<HubspotSettings>> CreateMockedConfiguration(bool withApiKey = true)
    {
        var mockedConfiguration = new Mock<IOptions<HubspotSettings>>();
        mockedConfiguration.Setup(x => x.Value).Returns(new HubspotSettings { ApiKey = withApiKey ? ApiKey : String.Empty });
        
        return mockedConfiguration;
    }

    private static HttpClient CreateMockedHttpClient(HttpStatusCode statusCode, string responseContent = "")
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(responseContent),
        };

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        var httpClient = new HttpClient(handlerMock.Object);
        return httpClient;
    }
}