using System;

namespace CV_Heroku
{
    public partial class WebForm : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Label1.Text = Convert.ToString(CrowdVision.PodMonitorJob.timesRan);

            Label2.Text = CrowdVision.App_Start.PodMonitorConfig.outText.ToString();
        }
    }
}