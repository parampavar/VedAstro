﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker.Http;
using System.Xml.Linq;
using VedAstro.Library;

namespace API
{
    /// <summary>
    /// A collection of general tools used by API
    /// </summary>
    public static partial class APITools
    {

        /// <summary>
        /// Gets main person list xml doc file
        /// </summary>
        /// <returns></returns>
        private static async Task<XDocument> GetPersonListFile()
        {
            var personListXml = await GetXmlFileFromAzureStorage(PersonListFile, BlobContainerName);

            return personListXml;
        }

        /// <summary>
        /// Gets main Saved Match Report list xml doc file
        /// </summary>
        /// <returns></returns>
        private static async Task<XDocument> GetSavedMatchReportListFile()
        {
            var savedMatchReportListXml = await GetXmlFileFromAzureStorage(SavedMatchReportList, BlobContainerName);

            return savedMatchReportListXml;
        }

    }
}