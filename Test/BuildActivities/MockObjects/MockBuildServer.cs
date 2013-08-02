using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Build.Client;

namespace Coldwater.Construct.Build.Activities.Test
{
    public class MockBuildServer : IBuildServer
    {
        /// <summary>
        /// Gets the standard CWS project collection.
        /// </summary>
        public Microsoft.TeamFoundation.Client.TfsTeamProjectCollection TeamProjectCollection
        {
            get
            {
                return new TfsTeamProjectCollection(new Uri("https://tfs.coldwatersoftware.com/tfs/coldwater"));
            }
        }

        #region Not Implemented

        public void AddBuildQuality(string teamProject, string[] qualities)
        {
            throw new NotImplementedException();
        }

        public void AddBuildQuality(string teamProject, string quality)
        {
            throw new NotImplementedException();
        }

        public BuildServerVersion BuildServerVersion
        {
            get { throw new NotImplementedException(); }
        }

        public void CancelBuilds(int[] ids)
        {
            throw new NotImplementedException();
        }

        public void CancelBuilds(IQueuedBuild[] builds)
        {
            throw new NotImplementedException();
        }

        public IBuildAgent CreateBuildAgent(string teamProject)
        {
            throw new NotImplementedException();
        }

        public IBuildAgentSpec CreateBuildAgentSpec(string teamProject, string agentName, string machineName, int port)
        {
            throw new NotImplementedException();
        }

        public IBuildAgentSpec CreateBuildAgentSpec(string teamProject, string agentName)
        {
            throw new NotImplementedException();
        }

        public IBuildAgentSpec CreateBuildAgentSpec(string teamProject)
        {
            throw new NotImplementedException();
        }

        public IBuildAgentSpec CreateBuildAgentSpec(IBuildAgent agent)
        {
            throw new NotImplementedException();
        }

        public IBuildAgentSpec CreateBuildAgentSpec(string name, string computer, System.Collections.Generic.List<string> tags)
        {
            throw new NotImplementedException();
        }

        public IBuildAgentSpec CreateBuildAgentSpec()
        {
            throw new NotImplementedException();
        }

        public IBuildControllerSpec CreateBuildControllerSpec(string name, string computer)
        {
            throw new NotImplementedException();
        }

        public IBuildControllerSpec CreateBuildControllerSpec()
        {
            throw new NotImplementedException();
        }

        public IBuildDefinition CreateBuildDefinition(string teamProject)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinitionSpec CreateBuildDefinitionSpec(string teamProject, string definitionName)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinitionSpec CreateBuildDefinitionSpec(string teamProject)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinitionSpec CreateBuildDefinitionSpec(IBuildDefinition definition)
        {
            throw new NotImplementedException();
        }

        public IBuildDetailSpec CreateBuildDetailSpec(System.Collections.Generic.IEnumerable<Uri> definitionUris)
        {
            throw new NotImplementedException();
        }

        public IBuildDetailSpec CreateBuildDetailSpec(IBuildDefinition definition)
        {
            throw new NotImplementedException();
        }

        public IBuildDetailSpec CreateBuildDetailSpec(IBuildDefinitionSpec definitionSpec)
        {
            throw new NotImplementedException();
        }

        public IBuildDetailSpec CreateBuildDetailSpec(string teamProject, string definitionName)
        {
            throw new NotImplementedException();
        }

        public IBuildDetailSpec CreateBuildDetailSpec(string teamProject)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuildSpec CreateBuildQueueSpec(System.Collections.Generic.IEnumerable<Uri> definitionUris)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuildSpec CreateBuildQueueSpec(string teamProject, string definitionName)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuildSpec CreateBuildQueueSpec(string teamProject)
        {
            throw new NotImplementedException();
        }

        public IBuildRequest CreateBuildRequest(Uri buildDefinitionUri, Uri buildControllerUri)
        {
            throw new NotImplementedException();
        }

        public IBuildRequest CreateBuildRequest(Uri buildDefinitionUri)
        {
            throw new NotImplementedException();
        }

        public IBuildServiceHost CreateBuildServiceHost(string name, Uri baseUrl)
        {
            throw new NotImplementedException();
        }

        public IBuildServiceHost CreateBuildServiceHost(string name, string scheme, string host, int port)
        {
            throw new NotImplementedException();
        }

        public IProcessTemplate CreateProcessTemplate(string teamProject, string serverPath)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuildsView CreateQueuedBuildsView(System.Collections.Generic.IEnumerable<Uri> definitionUris)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuildsView CreateQueuedBuildsView(string teamProject)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuildsView CreateQueuedBuildsView()
        {
            throw new NotImplementedException();
        }

        public void CreateTeamProjectComponents(Uri projectUri, System.Collections.Generic.IEnumerable<BuildTeamProjectPermission> permissions)
        {
            throw new NotImplementedException();
        }

        public void DeleteBuildAgents(Uri[] agentUris)
        {
            throw new NotImplementedException();
        }

        public void DeleteBuildAgents(IBuildAgent[] agents)
        {
            throw new NotImplementedException();
        }

        public void DeleteBuildControllers(Uri[] controllerUris)
        {
            throw new NotImplementedException();
        }

        public void DeleteBuildControllers(IBuildController[] controllers)
        {
            throw new NotImplementedException();
        }

        public void DeleteBuildDefinitions(Uri[] definitionUris)
        {
            throw new NotImplementedException();
        }

        public void DeleteBuildDefinitions(IBuildDefinition[] definitions)
        {
            throw new NotImplementedException();
        }

        public void DeleteBuildQuality(string teamProject, string[] qualities)
        {
            throw new NotImplementedException();
        }

        public void DeleteBuildQuality(string teamProject, string quality)
        {
            throw new NotImplementedException();
        }

        public void DeleteBuildServiceHost(Uri serviceHostUri)
        {
            throw new NotImplementedException();
        }

        public IBuildDeletionResult[] DeleteBuilds(Uri[] buildUris, DeleteOptions options)
        {
            throw new NotImplementedException();
        }

        public IBuildDeletionResult[] DeleteBuilds(Uri[] buildUris)
        {
            throw new NotImplementedException();
        }

        public IBuildDeletionResult[] DeleteBuilds(IBuildDetail[] builds, DeleteOptions options)
        {
            throw new NotImplementedException();
        }

        public IBuildDeletionResult[] DeleteBuilds(IBuildDetail[] builds)
        {
            throw new NotImplementedException();
        }

        public void DeleteProcessTemplates(IProcessTemplate[] processTemplates)
        {
            throw new NotImplementedException();
        }

        public void DestroyBuilds(Uri[] buildUris)
        {
            throw new NotImplementedException();
        }

        public void DestroyBuilds(IBuildDetail[] builds)
        {
            throw new NotImplementedException();
        }

        public void EvaluateSchedules()
        {
            throw new NotImplementedException();
        }

        public IBuildDefinition[] GetAffectedBuildDefinitions(string[] serverItems, ContinuousIntegrationType continuousIntegrationType)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinition[] GetAffectedBuildDefinitions(string[] serverItems)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail GetAllBuildDetails(Uri buildUri)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail GetBuild(IBuildDefinitionSpec buildDefinitionSpec, string buildNumber, string[] informationTypes, QueryOptions queryOptions)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail GetBuild(Uri buildUri, string[] informationTypes, QueryOptions queryOptions)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail GetBuild(Uri buildUri)
        {
            throw new NotImplementedException();
        }

        public IBuildAgent GetBuildAgent(Uri buildAgentUri)
        {
            throw new NotImplementedException();
        }

        public IBuildController GetBuildController(string name)
        {
            throw new NotImplementedException();
        }

        public IBuildController GetBuildController(Uri buildControllerUri, bool includeAgents)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinition GetBuildDefinition(string teamProject, string name, QueryOptions options)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinition GetBuildDefinition(string teamProject, string name)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinition GetBuildDefinition(Uri buildDefinitionUri, QueryOptions options)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinition GetBuildDefinition(Uri buildDefinitionUri)
        {
            throw new NotImplementedException();
        }

        public string[] GetBuildQualities(string teamProject)
        {
            throw new NotImplementedException();
        }

        public IBuildServiceHost GetBuildServiceHost(Uri buildServiceHostUri)
        {
            throw new NotImplementedException();
        }

        public IBuildServiceHost GetBuildServiceHost(string serviceHostName)
        {
            throw new NotImplementedException();
        }

        public string GetDisplayText(object value)
        {
            throw new NotImplementedException();
        }

        public string[] GetDisplayTextValues(Type enumType)
        {
            throw new NotImplementedException();
        }

        public object GetEnumValue(Type enumType, string displayText, object defaultValue)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail GetMinimalBuildDetails(Uri buildUri)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuild[] GetQueuedBuild(int[] queuedBuildIds, QueryOptions queryOptions)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuild GetQueuedBuild(int queuedBuildId, QueryOptions queryOptions)
        {
            throw new NotImplementedException();
        }

        public ScheduleDays GetScheduleDaysFromDaysOfWeek(DayOfWeek[] weekdays)
        {
            throw new NotImplementedException();
        }

        public string NoCICheckInComment
        {
            get { throw new NotImplementedException(); }
        }

        public void ProcessChangeset(int id)
        {
            throw new NotImplementedException();
        }

        public IBuildAgentQueryResult[] QueryBuildAgents(IBuildAgentSpec[] buildAgentSpecs)
        {
            throw new NotImplementedException();
        }

        public IBuildAgentQueryResult QueryBuildAgents(IBuildAgentSpec buildAgentSpec)
        {
            throw new NotImplementedException();
        }

        public IBuildAgent[] QueryBuildAgents(string teamProject)
        {
            throw new NotImplementedException();
        }

        public IBuildAgent[] QueryBuildAgentsByUri(Uri[] buildAgentUris)
        {
            throw new NotImplementedException();
        }

        public IBuildControllerQueryResult[] QueryBuildControllers(IBuildControllerSpec[] buildControllerSpecs)
        {
            throw new NotImplementedException();
        }

        public IBuildControllerQueryResult QueryBuildControllers(IBuildControllerSpec buildControllerSpec)
        {
            throw new NotImplementedException();
        }

        public IBuildController[] QueryBuildControllers(bool includeAgents)
        {
            throw new NotImplementedException();
        }

        public IBuildController[] QueryBuildControllers()
        {
            throw new NotImplementedException();
        }

        public IBuildController[] QueryBuildControllersByUri(Uri[] buildControllerUris, bool includeAgents)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinitionQueryResult[] QueryBuildDefinitions(IBuildDefinitionSpec[] buildDefinitionSpec)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinitionQueryResult QueryBuildDefinitions(IBuildDefinitionSpec buildDefinitionSpec)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinition[] QueryBuildDefinitions(string teamProject, QueryOptions options)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinition[] QueryBuildDefinitions(string teamProject)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinition[] QueryBuildDefinitionsByUri(Uri[] buildDefinitionUris, QueryOptions options)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinition[] QueryBuildDefinitionsByUri(Uri[] buildDefinitionUris)
        {
            throw new NotImplementedException();
        }

        public IBuildServiceHost[] QueryBuildServiceHosts(string serviceHostName)
        {
            throw new NotImplementedException();
        }

        public IBuildServiceHost[] QueryBuildServiceHostsByUri(Uri[] buildServiceHostUris)
        {
            throw new NotImplementedException();
        }

        public IBuildQueryResult[] QueryBuilds(IBuildDetailSpec[] buildDetailSpecs)
        {
            throw new NotImplementedException();
        }

        public IBuildQueryResult QueryBuilds(IBuildDetailSpec buildDetailSpec)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail[] QueryBuilds(IBuildDefinition definition)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail[] QueryBuilds(IBuildDefinitionSpec definitionSpec)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail[] QueryBuilds(string teamProject, string definitionName)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail[] QueryBuilds(string teamProject)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail[] QueryBuildsByUri(Uri[] buildUris, string[] informationTypes, QueryOptions queryOptions, QueryDeletedOption queryDeletedOption)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail[] QueryBuildsByUri(Uri[] buildUris, string[] informationTypes, QueryOptions queryOptions)
        {
            throw new NotImplementedException();
        }

        public IProcessTemplate[] QueryProcessTemplates(string teamProject, ProcessTemplateType[] types)
        {
            throw new NotImplementedException();
        }

        public IProcessTemplate[] QueryProcessTemplates(string teamProject)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuildQueryResult[] QueryQueuedBuilds(IQueuedBuildSpec[] buildQueueSpecs)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuildQueryResult QueryQueuedBuilds(IQueuedBuildSpec buildQueueSpec)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuild QueueBuild(IBuildRequest request, QueueOptions options)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuild QueueBuild(IBuildRequest request)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuild QueueBuild(IBuildDefinition definition)
        {
            throw new NotImplementedException();
        }
        
        IQueuedBuild[] QueueBuild(IEnumerable<IBuildRequest> requests, QueueOptions options)
        {
            throw new NotImplementedException();
        }

        public IBuildAgent[] SaveBuildAgents(IBuildAgent[] agents)
        {
            throw new NotImplementedException();
        }

        public IBuildController[] SaveBuildControllers(IBuildController[] controllers)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinition[] SaveBuildDefinitions(IBuildDefinition[] definitions)
        {
            throw new NotImplementedException();
        }

        public void SaveBuildServiceHost(IBuildServiceHost serviceHost)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail[] SaveBuilds(IBuildDetail[] builds)
        {
            throw new NotImplementedException();
        }

        public IProcessTemplate[] SaveProcessTemplates(IProcessTemplate[] processTemplates)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuild[] SaveQueuedBuilds(IQueuedBuild[] queuedBuilds)
        {
            throw new NotImplementedException();
        }

        public void StopBuilds(Uri[] uris)
        {
            throw new NotImplementedException();
        }

        public void StopBuilds(IBuildDetail[] builds)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuild[] StartQueuedBuildsNow(int[] builds)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuild[] StartQueuedBuildsNow(IQueuedBuild[] builds)
        {
            throw new NotImplementedException();
        }


        public Microsoft.TeamFoundation.Client.TeamFoundationServer TeamFoundationServer
        {
            get { throw new NotImplementedException(); }
        }

        

        public void TestConnectionForBuildAgent(IBuildAgent agent)
        {
            throw new NotImplementedException();
        }

        public void TestConnectionForBuildController(IBuildController controller)
        {
            throw new NotImplementedException();
        }

        public void TestConnectionsForBuildMachine(IBuildServiceHost host)
        {
            throw new NotImplementedException();
        }

        #endregion


        public IAsyncResult BeginQueryBuildDefinitions(IBuildDefinitionSpec[] specs, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginQueryBuildDefinitionsByUri(Uri[] uris, string[] propertyNameFilters, QueryOptions options, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginQueryBuilds(IBuildDetailSpec[] specs, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginQueryBuildsByUri(Uri[] buildUris, string[] informationTypes, QueryOptions queryOptions, QueryDeletedOption queryDeletedOption, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginQueryQueuedBuilds(IQueuedBuildSpec[] specs, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IBuildAgentSpec CreateBuildAgentSpec(string name, string computer, string[] propertyNameFilters, List<string> tags)
        {
            throw new NotImplementedException();
        }

        public IBuildControllerSpec CreateBuildControllerSpec(string name, string computer, string[] propertyNameFilters, bool includeAgents)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinitionSpec CreateBuildDefinitionSpec(string teamProject, string definitionName, string[] propertyNameFilters)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinitionQueryResult[] EndQueryBuildDefinitions(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinition[] EndQueryBuildDefinitionsByUri(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IBuildQueryResult[] EndQueryBuilds(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail[] EndQueryBuildsByUri(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuildQueryResult[] EndQueryQueuedBuilds(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinition[] GetAffectedBuildDefinitions(string[] serverItems, DefinitionTriggerType triggerType)
        {
            throw new NotImplementedException();
        }

        public IBuildDetail GetBuild(Uri buildUri, string[] informationTypes, QueryOptions queryOptions, QueryDeletedOption queryDeletedOption)
        {
            throw new NotImplementedException();
        }

        public IBuildAgent GetBuildAgent(Uri buildAgentUri, string[] propertyNameFilters)
        {
            throw new NotImplementedException();
        }

        public IBuildController GetBuildController(Uri buildControllerUri, string[] propertyNameFilters, bool includeAgents)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinition GetBuildDefinition(Uri buildDefinitionUri, string[] propertyNameFilters, QueryOptions options)
        {
            throw new NotImplementedException();
        }

        public IBuildAgent[] QueryBuildAgentsByUri(Uri[] buildAgentUris, string[] propertyNameFilters)
        {
            throw new NotImplementedException();
        }

        public IBuildController[] QueryBuildControllersByUri(Uri[] buildControllerUris, string[] propertyNameFilters, bool includeAgents)
        {
            throw new NotImplementedException();
        }

        public IBuildDefinition[] QueryBuildDefinitionsByUri(Uri[] buildDefinitionUris, string[] propertyNameFilters, QueryOptions options)
        {
            throw new NotImplementedException();
        }

        IQueuedBuild[] IBuildServer.QueueBuild(IEnumerable<IBuildRequest> requests, QueueOptions options)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuild[] RetryQueuedBuilds(IQueuedBuild[] queuedBuilds, Guid batchId, QueuedBuildRetryOption retryOption)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuild[] RetryQueuedBuilds(IQueuedBuild[] queuedBuilds, Guid batchId)
        {
            throw new NotImplementedException();
        }

        public IQueuedBuild[] RetryQueuedBuilds(IQueuedBuild[] queuedBuilds)
        {
            throw new NotImplementedException();
        }
    }
}
