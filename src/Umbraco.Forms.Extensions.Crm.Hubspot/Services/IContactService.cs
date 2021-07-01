﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Extensions.Crm.Hubspot.Models;
using Umbraco.Forms.Extensions.Crm.Hubspot.Models.Responses;

namespace Umbraco.Forms.Extensions.Crm.Hubspot.Services
{
    public enum CommandResult
    {
        Success,
        Failed,
        NotConfigured,
    }

    public interface IContactService
    {
        Task<IEnumerable<Property>> GetContactProperties();

        Task<CommandResult> PostContact(Record record, List<MappedProperty> fieldMappings);
    }
}
