//==============================================================================
// Copyright (c) NT Prime LLC. All Rights Reserved.
//==============================================================================

using System;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace Construct.Build.TfsServices
{
    public static class ServiceClientFactory
    {
        #region consts

        internal static readonly Uri DefaultIdentityManagmentServiceRelativeUrl = new Uri("_tfs_resources/Services/v3.0/IdentityManagementService.asmx", UriKind.Relative);
        internal static readonly Uri DefaultTestResultsServiceRelativeUrl = new Uri("_tfs_resources/TestManagement/v1.0/TestResults.asmx", UriKind.Relative);

        #endregion

        #region properties

        public static Uri TfsUrl
        {
            get;
            set;
        }

        public static Uri TfsTeamCollectionUrl
        {
            get;
            set;
        }

        public static Uri IdentityManagementServiceRelativeUrl
        {
            get; 
            set;
        }

        public static Uri TestResultsServiceRelativeUrl
        {
            get;
            set;
        }

        #endregion

        #region methods

        //public static IdentityManagementWebServiceSoapClient CreateIdentityManagementWebServiceSoapClient(Uri relativeUrl)
        //{
        //    if (relativeUrl == null)
        //    {
        //        throw new ArgumentNullException("relativeUrl");
        //    }

        //    BasicHttpBinding binding = ServiceClientFactory.CreateBinding();
        //    EndpointAddress remoteAddress =
        //        ServiceClientFactory.CreateEndpoint(new Uri(ServiceClientFactory.TfsTeamCollectionUrl, relativeUrl));
        //    return new IdentityManagementWebServiceSoapClient(binding, remoteAddress);

        //}

        //public static IdentityManagementWebServiceSoapClient CreateIdentityManagementWebServiceSoapClient()
        //{
        //    if (IdentityManagementServiceRelativeUrl == null)
        //    {
        //        ServiceClientFactory.IdentityManagementServiceRelativeUrl 
        //            = ServiceClientFactory.DefaultIdentityManagmentServiceRelativeUrl;
        //    }

        //    return CreateIdentityManagementWebServiceSoapClient(ServiceClientFactory.IdentityManagementServiceRelativeUrl);
        //}
        
        //public static TestResultsServiceSoapClient CreateTestResultsServiceSoapClient(Uri relativeUrl )
        //{
        //    if (relativeUrl == null)
        //    {
        //        throw new ArgumentNullException("relativeServiceUrl");
        //    }

        //    BasicHttpBinding binding = ServiceClientFactory.CreateBinding();
        //    EndpointAddress remoteAddress = 
        //        ServiceClientFactory.CreateEndpoint(new Uri(ServiceClientFactory.TfsTeamCollectionUrl,relativeUrl));
        //    return new TestResultsServiceSoapClient(binding, remoteAddress);
        //}

        ///// <summary>
        ///// Creates a new soap client using the relative service path currently assigned to the factory property.
        ///// </summary>
        ///// <returns></returns>
        //public static TestResultsServiceSoapClient CreateTestResultsServiceSoapClient()
        //{
        //    if (TestResultsServiceRelativeUrl == null)
        //    {
        //        ServiceClientFactory.TestResultsServiceRelativeUrl
        //            = ServiceClientFactory.DefaultTestResultsServiceRelativeUrl;
        //    }

        //    return CreateTestResultsServiceSoapClient(ServiceClientFactory.TestResultsServiceRelativeUrl);
        //}

        /// <summary>
        /// Builds a <see cref="BasicHttpBinding"/> with Ntml auth and transport credentials.
        /// </summary>
        /// <returns>A newly created <see cref="BasicHttpBinding"/>.</returns>
        private static BasicHttpBinding CreateBinding()
        {
            var binding =  new BasicHttpBinding
            {
                Security = new BasicHttpSecurity
                {
                    Mode = BasicHttpSecurityMode.TransportCredentialOnly,
                    Message = new BasicHttpMessageSecurity
                    {
                        AlgorithmSuite = SecurityAlgorithmSuite.Default,
                        ClientCredentialType = BasicHttpMessageCredentialType.UserName
                    },
                    Transport = new HttpTransportSecurity
                    {
                        ClientCredentialType = HttpClientCredentialType.Ntlm,
                        ProxyCredentialType = HttpProxyCredentialType.None
                    }
                }
            };

            return binding;
        }
        
        /// <summary>
        /// Creates a new <see cref="EndpointAddress"/> from the incoming relative Url and the TFS server url.
        /// </summary>
        /// <param name="relativeUrl">The relative service Url.</param>
        /// <returns>A new <see cref="EndpointAddress"/>.</returns>
        private static EndpointAddress CreateEndpoint(Uri serviceUrl)
        {
            return new EndpointAddress(serviceUrl);
        }

        #endregion
    }
}