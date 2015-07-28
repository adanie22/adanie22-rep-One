using System.Configuration;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MWS.CUIT.AppControls;
using MWS.SharePoint.CUIT.Pages;
using System;
using System.Threading;
using MWS.SharePoint.CUIT.Utilitiy;
using MWS.CUIT.AppControls.WebControls;
using Microsoft.VisualStudio.TestTools.UITest.Extension;

namespace MWS.SharePoint.CUIT
{
    /// <summary>
    /// Contains App tests for:
    /// - Editing an existing workflow attached to a content type
    /// - Deleting an existing workflow attached to a content type
    /// </summary>
    [CodedUITest]
    public class ManageWorkflow : Base
    {
        private string environment = ConfigurationManager.AppSettings["TeamSiteURL"];
        private string workflowNamePrefix = ConfigurationManager.AppSettings["WorkflowNamePrefix"];

        ManageWorkflowPage workflowPage;

        #region XML Comments
        /// <summary>
        /// Verify the user is able to Edit Workflow. 
        /// Test summary details are:
        /// <list type="bullet">
        /// <item>
        /// <description>Test Case Number: BS_TC011</description>
        /// </item>
        /// <item>
        /// <description>Requirement Number: BS4</description>
        /// </item>
        /// <item>
        /// <description>Requirement: Activity Workflow</description>
        /// </item>
        /// <item>
        /// <description>Offering: Standard</description>
        /// </item>
        /// </list>
        /// The test steps are:
        /// <list type="number">
        /// <item>
        /// <description>Create a workflow</description>
        /// </item>
        /// <item>
        /// <description>Edit a workflow</description>
        /// </item>
        /// <item>
        /// <description>Test workflow was successfully edited</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestCategory("Activity Workflow")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC011")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void EditContentTypeWorkFlowAsSiteOwner()
        {
            string manageContentTypeURL = string.Format("{0}_layouts/15/mngctype.aspx", environment);
            string workflowName = workflowNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            string contentType = "Comment";

            workflowPage = new ManageWorkflowPage(environment);
            bool workflowCreatedSuccessfully = workflowPage.CreateWorkflow(manageContentTypeURL, workflowName, contentType);

            if (workflowCreatedSuccessfully)
            {
                Console.WriteLine(string.Format("The workflow:{0} app WAS created on content type:{1}.", workflowName, contentType));
                bool workflowEditedSuccessfully = workflowPage.EditContentTypeWorkFlow(manageContentTypeURL, workflowName, contentType);
                    
                Assert.AreEqual(workflowEditedSuccessfully, true, string.Format("The workflow:{0} app WAS NOT edited on content type:{1}.", workflowName, contentType));

                // delete the workflow as having one may break the delete workflow test
                if (workflowCreatedSuccessfully)
                {
                    bool workflowDeletedSuccessfully = workflowPage.DeleteWorkFlow(manageContentTypeURL, workflowName, contentType);
                }
            }
            else
            {
               Assert.AreEqual(workflowCreatedSuccessfully, true, string.Format("The workflow:{0} app WAS NOT created on content type:{1}.", workflowName, contentType));
            }

        }

        #region XML Comments
        /// <summary>
        /// Remove an approval workflow from a list.
        /// Test summary details are:
        /// <list type="bullet">
        /// <item>
        /// <description>Test Case Number: BS_TC056</description>
        /// </item>
        /// <item>
        /// <description>Requirement Number: BS4</description>
        /// </item>
        /// <item>
        /// <description>Requirement: Activity Workflow</description>
        /// </item>
        /// <item>
        /// <description>Offering: Standard</description>
        /// </item>
        /// </list>
        /// The test steps are:
        /// <list type="number">
        /// <item>
        /// <description>Create a workflow</description>
        /// </item>
        /// <item>
        /// <description>Delete a workflow</description>
        /// </item>
        /// <item>
        /// <description>Test workflow was successfully deleted</description>
        /// </item>
        /// </list>
        /// </summary>
        #endregion
        [TestCategory("Activity Workflow")]
        [TestCategory("Standard")]
        [TestCategory("BS_TC056")]
        [TestCategory("TeamSite")]
        [TestMethod, Timeout(TestTimeout.Infinite)]
        public void DeleteContentTypeWorkFlowAsSiteOwner()
        {
            string manageContentTypeURL = string.Format("{0}_layouts/15/mngctype.aspx", environment);
            string workflowName = workflowNamePrefix + DateTime.Now.ToString("yyyMMddHHss");
            string contentType = "Comment";
            workflowPage = new ManageWorkflowPage(environment);

            bool workflowCreatedSuccessfully = workflowPage.CreateWorkflow(manageContentTypeURL, workflowName, contentType);

            if (workflowCreatedSuccessfully)
            {
                Console.WriteLine(string.Format("The workflow:{0} app WAS created on content type:{1}.", workflowName, contentType));
                bool workflowDeletedSuccessfully = workflowPage.DeleteWorkFlow(manageContentTypeURL, workflowName, contentType);

                Assert.AreEqual(workflowDeletedSuccessfully, true, string.Format("The workflow:{0} app WAS NOT deleted on content type:{1}.", workflowName, contentType));
            }
            else
            {
                Assert.AreEqual(workflowCreatedSuccessfully, true, string.Format("The workflow:{0} app WAS NOT created on content type:{1}.", workflowName, contentType));
            }

        }

    }
}
