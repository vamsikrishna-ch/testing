using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using System.Xml;
using System.Xml.Serialization;

namespace UTI_InstaRedemption.Controllers
{
    public class StatusController : ApiController
    {
          APIBanking.Environment env = new APIBanking.Environments.YBL.UAT("2449810", "Yesbank1", "7a7a26d8-1679-436b-854a-a2b5682bbf11", "nP8oE0tO5wR5kI1qD3aA6aR6wD6hR7hB8oP6qW5vU0hN0wE4sD", null);
       // APIBanking.Environment env = new APIBanking.Environments.YBL.UAT(ConfigurationManager.AppSettings["customerId"].ToString(), ConfigurationManager.AppSettings["Password"].ToString(), ConfigurationManager.AppSettings["clientId"].ToString(), ConfigurationManager.AppSettings["clientSecret"].ToString(), ConfigurationManager.AppSettings["CertificatePath"].ToString(), "123");
        string message = "";
        Common c = new Common();
        [HttpPost]
        public HttpResponseMessage GetStatus(string appId, string customerId, string requestReferenceNo)
        {
            DataSet ds = new DataSet();
            string URL = "Status/GetStatus&appId?" + appId + "&customerId?" + customerId + "&requestReferenceNo?" + requestReferenceNo + "";
            ds = c.getInserlogrequest(URL);
            com.getStatus gStatus = new com.getStatus();
            com.getStatusRequest gStatusRequest = new com.getStatusRequest();
            com.getStatusResponse gStatusResponse = new com.getStatusResponse();
            gStatus.version = "2.0";
            gStatus.appID = appId;
            gStatus.customerID = customerId;
            gStatus.requestReferenceNo = requestReferenceNo;
            try
            {
                gStatusResponse = APIBanking.DomesticRemittanceClient.getStatus(env, gStatus);
                StringWriter sw = new StringWriter();
                XmlTextWriter tw = null;
                XmlSerializer serializer = new XmlSerializer(gStatusResponse.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, gStatusResponse);
                string tes = sw.ToString();
                c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), tes.ToString());
                //c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), gStatusResponse.ToString());
                return this.Request.CreateResponse(HttpStatusCode.OK, gStatusResponse);
            }
            //catch (TimeoutException ex)
            //{
            //    message = ex.Message;
            //    HttpError myCustomError = new HttpError(message);
            //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, myCustomError);
            //}
            catch (FaultException ex)
            {
                String faultCode = ex.Code.SubCode.Name;
                String FaultReason = ex.Message;

                message = faultCode + " - " + FaultReason;

                HttpError myCustomError = new HttpError();
                myCustomError.Add("ErrorCode", faultCode);
                myCustomError.Add("Errormsg", FaultReason);
                myCustomError.Add("Ihno", requestReferenceNo);
                StringWriter sw = new StringWriter();
                XmlTextWriter tw = null;
                XmlSerializer serializer = new XmlSerializer(myCustomError.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, myCustomError);
                string tes = sw.ToString();
                c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), tes);
                //c.InsertResponse(faultCode, FaultReason, requestReferenceNo, "");
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, myCustomError);
            }
            //catch (CommunicationException ex)
            //{
            //    message = ex.Message;
            //    HttpError myCustomError = new HttpError(message);
            //    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, myCustomError);
            //}

            catch (Exception ex)
            {
                HttpError myCustomError = new HttpError();
                myCustomError.Add("ErrorCode", 500);
                myCustomError.Add("Errormsg", "InternerlServer Error");
                myCustomError.Add("Ihno", requestReferenceNo);
                StringWriter sw = new StringWriter();
                XmlTextWriter tw = null;
                XmlSerializer serializer = new XmlSerializer(myCustomError.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, myCustomError);
                string tes = sw.ToString();
                c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), tes);
                //c.InsertResponse("500", ex.Message, requestReferenceNo, "");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, myCustomError);
            }
        }
    }
}
