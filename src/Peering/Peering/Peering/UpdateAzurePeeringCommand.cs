﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Microsoft" file="UpdateAzurePeeringCommand.cs">
//   Licensed under the Apache License, Version 2.0 (the "License");
//   //   you may not use this file except in compliance with the License.
//   //   You may obtain a copy of the License at
//   //   http://www.apache.org/licenses/LICENSE-2.0
//   //   Unless required by applicable law or agreed to in writing, software
//   //   distributed under the License is distributed on an "AS IS" BASIS,
//   //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   //   See the License for the specific language governing permissions and
//   //   limitations under the License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Azure.PowerShell.Cmdlets.Peering.Peering
{
    using System;
    using System.Management.Automation;

    using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
    using Microsoft.Azure.Management.Peering;
    using Microsoft.Azure.Management.Peering.Models;
    using Microsoft.Azure.PowerShell.Cmdlets.Peering.Common;
    using Microsoft.Azure.PowerShell.Cmdlets.Peering.Models;

    /// <inheritdoc />
    /// <summary>
    ///     Updates the InputObject object.
    /// </summary>
    [Cmdlet(
        "Update",
        "AzPeering",
        DefaultParameterSetName = Constants.ParameterSetNameDefault + Constants.Exchange,
        SupportsShouldProcess = true)]
    [OutputType(typeof(PSPeering))]
    public class UpdateAzurePeeringCommand : PeeringBaseCmdlet
    {
        /// <summary>
        /// Gets or sets the input object.
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ParameterSetName = Constants.ParameterSetNameDefault + Constants.Direct,
            DontShow = true)]
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ParameterSetName = Constants.ParameterSetNameDefault + Constants.Exchange,
            DontShow = true)]
        public PSPeering InputObject { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = Constants.ParameterSetNameByResourceId + Constants.Direct,
            HelpMessage = Constants.ResourceIdHelp)]
        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = Constants.ParameterSetNameByResourceId + Constants.Exchange,
            HelpMessage = Constants.ResourceIdHelp)]
        [ValidateNotNullOrEmpty]
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets The Resource Group Name
        /// </summary>
        [Parameter(
            Position = Constants.PositionPeeringZero,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = Constants.ResourceGroupNameHelp,
            ParameterSetName = Constants.Direct)]
        [Parameter(
            Position = Constants.PositionPeeringZero,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = Constants.ResourceGroupNameHelp,
            ParameterSetName = Constants.Exchange)]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public virtual string ResourceGroupName { get; set; }

        /// <summary>
        /// Gets or sets The InputObject NameMD5AuthenticationKeyHelp
        /// </summary>
        [Parameter(
            Position = Constants.PositionPeeringOne,
            Mandatory = true,
            HelpMessage = Constants.PeeringNameHelp,
            ValueFromPipelineByPropertyName = true,
            ParameterSetName = Constants.Direct)]
        [Parameter(
            Position = Constants.PositionPeeringOne,
            ValueFromPipelineByPropertyName = true,
            Mandatory = true,
            HelpMessage = Constants.PeeringNameHelp,
            ParameterSetName = Constants.Exchange)]
        [ValidateNotNullOrEmpty]
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether use for peering service.
        /// </summary>
        [Parameter(
            Mandatory = false,
            HelpMessage = Constants.UseForPeeringServiceHelp,
            ParameterSetName = Constants.Direct)]
        [Parameter(
            Mandatory = false,
            ParameterSetName = Constants.ParameterSetNameDefault + Constants.Direct,
            HelpMessage = Constants.UseForPeeringServiceHelp)]
        [Parameter(
            Mandatory = false,
            ParameterSetName = Constants.ParameterSetNameByResourceId + Constants.Direct,
            HelpMessage = Constants.UseForPeeringServiceHelp)]
        public bool UseForPeeringService { get; set; }

        /// <summary>
        /// Gets or sets the exchange session.
        /// </summary>
        [Parameter(
            Mandatory = true,
            Position = Constants.PositionPeeringTwo,
            ValueFromPipeline = true,
            HelpMessage = Constants.PeeringExchangeConnectionHelp,
            ParameterSetName = Constants.Exchange)]
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            Position = Constants.PositionPeeringOne,
            HelpMessage = Constants.PeeringExchangeConnectionHelp,
            ParameterSetName = Constants.ParameterSetNameByResourceId + Constants.Exchange)]
        [Parameter(
            Mandatory = false,
            HelpMessage = Constants.PeeringExchangeConnectionHelp,
            ParameterSetName = Constants.ParameterSetNameDefault + Constants.Exchange)]
        public virtual PSExchangeConnection[] ExchangeConnection { get; set; }

        /// <summary>
        /// Gets or sets the direct session.
        /// </summary>
        [Parameter(
            Mandatory = false,
            ValueFromPipeline = true,
            HelpMessage = Constants.PeeringDirectConnectionHelp,
            ParameterSetName = Constants.Direct)]
        [Parameter(
            Mandatory = false,
            ValueFromPipeline = true,
            HelpMessage = Constants.PeeringDirectConnectionHelp,
            ParameterSetName = Constants.ParameterSetNameByResourceId + Constants.Direct)]
        [Parameter(
            Mandatory = false,
            HelpMessage = Constants.PeeringDirectConnectionHelp,
            ParameterSetName = Constants.ParameterSetNameDefault + Constants.Direct)]
        public virtual PSDirectConnection[] DirectConnection { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     The base execute operation.
        /// </summary>
        public override void Execute()
        {
            try
            {
                base.Execute();
                if (this.ParameterSetName.Contains(Constants.ParameterSetNameDefault))
                {
                    if (this.InputObject is PSDirectPeeringModelView)
                    {
                        var peeringRequest = this.InputUpdateDirect();
                        this.WriteObject(peeringRequest);
                    }

                    if (this.InputObject is PSExchangePeeringModelView)
                    {
                        var peeringRequest = this.InputUpdateExchange();
                        if (this.UseForPeeringService)
                            this.WriteVerbose("UseForPeeringService is only offered for Direct Peerings.");
                        this.WriteObject(peeringRequest);
                    }
                }

                if (this.ParameterSetName.Contains(Constants.ParameterSetNameByResourceId))
                {
                    var resourceGroupName = this.GetResourceGroupNameFromId(this.ResourceId).ToString();
                    var resourceName = this.GetPeeringNameFromId(this.ResourceId).ToString();
                    var peeringRequest = this.PeeringClient.Get(
                        resourceGroupName,
                        resourceName);
                    var peering = this.ToPeeringPs(peeringRequest);
                    if (peering.Exchange == null)
                    {
                       // var directPeering = new PSDirectPeeringModelView(peering);
                        this.WriteObject(this.UpdateDirectPeering(peering, resourceGroupName, resourceName));
                    }

                    if (peering.Direct == null)
                    {
                       // var exchangePeering = new PSExchangePeeringModelView(peering);
                        this.WriteObject(this.UpdateExchangePeering(peering, resourceGroupName, resourceName));
                    }
                }

                if (this.ParameterSetName.Equals(Constants.Direct, StringComparison.OrdinalIgnoreCase))
                {
                    var peeringRequest = this.GetAndUpdateDirectPeering();
                    this.WriteObject(peeringRequest);
                }

                if (this.ParameterSetName.Equals(Constants.Exchange, StringComparison.OrdinalIgnoreCase))
                {
                    var peeringRequest = this.GetAndUpdateExchangePeering();
                    this.WriteObject(peeringRequest);
                }
            }
            catch (InvalidOperationException mapException)
            {
                throw new InvalidOperationException($"Failed to map object {mapException}");
            }
            catch (ErrorResponseException ex)
            {
                throw new ErrorResponseException(
                    $"Error:{ex.Response.ReasonPhrase} reason:{ex.Body?.Code} message:{ex.Body?.Message}");
            }
        }

        /// <summary>
        /// The input update direct.
        /// </summary>
        /// <returns>
        /// The <see cref="PSDirectPeeringModelView"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException"> Only one value can be changed.
        /// </exception>
        /// <exception cref="ErrorResponseException"> Http request failed.
        /// </exception>
        private PSDirectPeeringModelView InputUpdateDirect()
        {
            try
            {
                if (this.InputObject is PSDirectPeeringModelView directPeeringModelView)
                {
                    var resourceGroupName = this.GetResourceGroupNameFromId(directPeeringModelView.Id);
                    var peeringName = this.GetPeeringNameFromId(directPeeringModelView.Id);
                    var peering = new PSPeering
                                      {
                                          Kind = directPeeringModelView.Kind,
                                          PeeringLocation = directPeeringModelView.PeeringLocation,
                                          Location = directPeeringModelView.Location,
                                          Sku = directPeeringModelView.Sku,
                                          Tags = directPeeringModelView.Tags,
                                          Direct = new PSPeeringPropertiesDirect
                                                       {
                                                           Connections = directPeeringModelView.Connections,
                                                           PeerAsn = new PSSubResource(
                                                               directPeeringModelView.PeerAsn.Id),
                                                           UseForPeeringService = directPeeringModelView
                                                               .UseForPeeringService
                                                       }
                                      };
                    peering.Sku = this.UseForPeeringService
                                      ? new PSPeeringSku { Name = Constants.PremiumDirectFree }
                                      : new PSPeeringSku { Name = Constants.BasicDirectFree };
                    peering.Direct.UseForPeeringService = this.UseForPeeringService;

                    return new PSDirectPeeringModelView(
                        this.ToPeeringPs(
                            this.PeeringClient.CreateOrUpdate(
                                resourceGroupName.ToString(),
                                peeringName.ToString(),
                                this.ToPeering(peering))));
                }
            }
            catch (InvalidOperationException mapException)
            {
                throw new InvalidOperationException($"Failed to map object {mapException}");
            }
            catch (ErrorResponseException ex)
            {
                throw new ErrorResponseException(
                    $"Error:{ex.Response.ReasonPhrase} reason:{ex.Body?.Code} message:{ex.Body?.Message}");
            }

            throw new InvalidOperationException("Check the input parameters.");
        }

        /// <summary>
        /// The input update exchange.
        /// </summary>
        /// <returns>
        /// The <see cref="PSExchangePeeringModelView"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException"> Only one value can be changed.
        /// </exception>
        /// <exception cref="ErrorResponseException"> Http request failed.
        /// </exception>
        private PSExchangePeeringModelView InputUpdateExchange()
        {
            try
            {
                if (this.InputObject is PSExchangePeeringModelView exchangePeeringModelView)
                {
                    var resourceGroupName = this.GetResourceGroupNameFromId(exchangePeeringModelView.Id);
                    var peeringName = this.GetPeeringNameFromId(exchangePeeringModelView.Id);
                    var peering = new PSPeering
                                      {
                                          Kind = exchangePeeringModelView.Kind,
                                          PeeringLocation = exchangePeeringModelView.PeeringLocation,
                                          Location = exchangePeeringModelView.Location,
                                          Sku = exchangePeeringModelView.Sku,
                                          Tags = exchangePeeringModelView.Tags,
                                          Exchange = new PSPeeringPropertiesExchange
                                                         {
                                                             Connections = exchangePeeringModelView.Connections,
                                                             PeerAsn = new PSSubResource(
                                                                 exchangePeeringModelView.PeerAsn.Id),
                                                         }
                                      };
                    this.PeeringClient.CreateOrUpdate(
                        resourceGroupName.ToString(),
                        peeringName.ToString(),
                        this.ToPeering(peering));

                    return new PSExchangePeeringModelView(
                        this.ToPeeringPs(this.PeeringClient.Get(resourceGroupName.ToString(), peeringName.ToString())));
                }
            }
            catch (InvalidOperationException mapException)
            {
                throw new InvalidOperationException($"Failed to map object {mapException}");
            }
            catch (ErrorResponseException ex)
            {
                throw new ErrorResponseException(
                    $"Error:{ex.Response.ReasonPhrase} reason:{ex.Body?.Code} message:{ex.Body?.Message}");
            }

            throw new InvalidOperationException("Check the input parameters.");
        }

        /// <summary>
        /// The get and update direct peering.
        /// </summary>
        /// <returns>
        /// The <see cref="PSDirectPeeringModelView"/>.
        /// </returns>
        private PSDirectPeeringModelView GetAndUpdateDirectPeering()
        {
            var directPeering = this.ToPeeringPs(this.PeeringClient.Get(this.ResourceGroupName, this.Name));
            directPeering.Sku = this.UseForPeeringService
                                    ? new PSPeeringSku { Name = Constants.PremiumDirectFree }
                                    : new PSPeeringSku { Name = Constants.BasicDirectFree };
            directPeering.Direct.UseForPeeringService = this.UseForPeeringService;
            directPeering.Direct.PeerAsn = new PSSubResource(directPeering.Direct.PeerAsn.Id);
            if (this.DirectConnection != null)
            {
                // check to see which connection has been changed
                for (int i = 0; i < directPeering.Direct.Connections.Count - 1; i++)
                {
                    var differences = directPeering.Direct.Connections[i].DetailedCompare(this.DirectConnection[i]);
                    if (differences.Count == 1)
                    {
                        directPeering.Direct.Connections[i] = this.DirectConnection[i];
                    }

                    if (differences.Count > 1) continue;
                    var newPeering = this.PeeringClient.CreateOrUpdate(
                        this.ResourceGroupName,
                        this.Name,
                        this.ToPeering(directPeering));
                    return new PSDirectPeeringModelView(this.ToPeeringPs(newPeering));
                }
            }

            return new PSDirectPeeringModelView(
                this.ToPeeringPs(
                    this.PeeringClient.CreateOrUpdate(
                        this.ResourceGroupName,
                        this.Name,
                        this.ToPeering(directPeering))));
        }

        /// <summary>
        /// The get and update direct peering.
        /// </summary>
        /// <param name="directPeering">
        /// The direct peering.
        /// </param>
        /// <returns>
        /// The <see cref="PSDirectPeeringModelView"/>.
        /// </returns>
        private PSDirectPeeringModelView UpdateDirectPeering(PSPeering directPeering, string resourceGroupName, string name)
        {
            directPeering.Direct.UseForPeeringService = this.UseForPeeringService;
            directPeering.Direct.PeerAsn = new PSSubResource(directPeering.Direct.PeerAsn.Id);
            if (this.DirectConnection != null)
            {
                // check to see which connection has been changed
                for (int i = 0; i < directPeering.Direct.Connections.Count - 1; i++)
                {
                    var differences = directPeering.Direct.Connections[i].DetailedCompare(this.DirectConnection[i]);
                    if (differences.Count == 1)
                    {
                        directPeering.Direct.Connections[i] = this.DirectConnection[i];
                    }

                    if (differences.Count > 1) continue;
                    var newPeering = this.PeeringClient.CreateOrUpdate(
                        resourceGroupName,
                        name,
                        this.ToPeering(directPeering));
                    return new PSDirectPeeringModelView(this.ToPeeringPs(newPeering));
                }
            }

            return new PSDirectPeeringModelView(
                this.ToPeeringPs(
                    this.PeeringClient.CreateOrUpdate(
                        this.ResourceGroupName,
                        this.Name,
                        this.ToPeering(directPeering))));
        }

        /// <summary>
        /// The get and update exchange peering.
        /// </summary>
        /// <returns>
        /// The <see cref="PSExchangePeeringModelView"/>.
        /// </returns>
        /// <exception cref="PSInvalidOperationException"> Only one value can be changed.
        /// </exception>
        private PSExchangePeeringModelView GetAndUpdateExchangePeering()
        {
            var exchangePeering = this.ToPeeringPs(this.PeeringClient.Get(this.ResourceGroupName, this.Name));
            exchangePeering.Exchange.PeerAsn = new PSSubResource(exchangePeering.Exchange.PeerAsn.Id);
            if (this.ExchangeConnection != null)
            {
                // check to see which connection has been changed
                for (int i = 0; i < exchangePeering.Exchange.Connections.Count - 1; i++)
                {
                    var differences = exchangePeering.Exchange.Connections[i]
                        .DetailedCompare(this.ExchangeConnection[i]);
                    if (differences.Count == 1)
                    {
                        exchangePeering.Exchange.Connections[i] = this.ExchangeConnection[i];
                    }

                    if (differences.Count > 1) continue;
                    return new PSExchangePeeringModelView(
                        this.ToPeeringPs(
                            this.PeeringClient.CreateOrUpdate(
                                this.ResourceGroupName,
                                this.Name,
                                this.ToPeering(exchangePeering))));
                }
            }

            throw new PSArgumentNullException("Exchange Connections is Null or Invalid.");
        }

        /// <summary>
        /// The update exchange peering.
        /// </summary>
        /// <param name="psExchange">
        /// The ps exchange.
        /// </param>
        /// <param name="resourceGroupName">
        /// The resource group name.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <returns>
        /// The <see cref="PSExchangePeeringModelView"/>.
        /// </returns>
        /// <exception cref="PSArgumentNullException"> Not valid operation
        /// </exception>
        private PSExchangePeeringModelView UpdateExchangePeering(PSPeering psExchange, string resourceGroupName, string name)
        {
            psExchange.Exchange.PeerAsn = new PSSubResource(psExchange.Exchange.PeerAsn.Id);
            if (this.ExchangeConnection != null)
            {
                // check to see which connection has been changed
                for (int i = 0; i < psExchange.Exchange.Connections.Count - 1; i++)
                {
                    var differences = psExchange.Exchange.Connections[i].DetailedCompare(this.ExchangeConnection[i]);
                    if (differences.Count == 1)
                    {
                        psExchange.Exchange.Connections[i] = this.ExchangeConnection[i];
                    }

                    if (differences.Count > 1) continue;
                    return new PSExchangePeeringModelView(
                        this.ToPeeringPs(
                            this.PeeringClient.CreateOrUpdate(
                                resourceGroupName,
                                name,
                                this.ToPeering(psExchange))));
                }
            }

            throw new PSArgumentNullException("Exchange Connections is Null or Invalid.");
        }
    }
}