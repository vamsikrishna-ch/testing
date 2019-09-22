using APIBanking;
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
using System.ServiceModel.Security;
using System.Web.Http;
using System.Xml;
using System.Xml.Serialization;
using UTI_InstaRedemption.com;

namespace UTI_InstaRedemption.Controllers
{
    public class BalanceController : ApiController
    {
        APIBanking.Environment env = new APIBanking.Environments.YBL.UAT("2449810", "Yesbank1", "7a7a26d8-1679-436b-854a-a2b5682bbf11", "nP8oE0tO5wR5kI1qD3aA6aR6wD6hR7hB8oP6qW5vU0hN0wE4sD", ConfigurationManager.AppSettings["CertificatePath"].ToString(), "123", null);
        //APIBanking.Environment env = new APIBanking.Environments.YBL.UAT(ConfigurationManager.AppSettings["customerId"].ToString(), ConfigurationManager.AppSettings["Password"].ToString(), ConfigurationManager.AppSettings["clientId"].ToString(), ConfigurationManager.AppSettings["clientSecret"].ToString(), ConfigurationManager.AppSettings["CertificatePath"].ToString(), "123");

        com.getBalance getBalanceRequest = new com.getBalance();
        com.getBalanceResponse getBalanceResponse = new com.getBalanceResponse();
        string message = "";
        Common c = new Common();
        [HttpPost]
        public HttpResponseMessage GetBalance(string version, string appId, string customerId, string AccountNumber)
        {
            DataSet ds = new DataSet();
            getBalanceRequest.version = version;
            getBalanceRequest.appID = appId;
            getBalanceRequest.customerID = customerId;
            getBalanceRequest.AccountNumber = AccountNumber;
            try
            {
               // DataSet ds = new DataSet();
                string URL = "Balance/GetBalance&version?" + version + "&customerId?" + customerId + "&AccountNumber?" + AccountNumber + "";
                ds = c.getInserlogrequest(URL);
                getBalanceResponse = APIBanking.DomesticRemittanceClient.getBalance(env, getBalanceRequest);
                StringWriter sw = new StringWriter();
                XmlTextWriter tw = null;
                XmlSerializer serializer = new XmlSerializer(getBalanceResponse.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, getBalanceResponse);
                string tes = sw.ToString();
                c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), tes.ToString());
               // c.writelog(e.Message, "FaultException", DateTime.Now, "", "");
                return this.Request.CreateResponse(HttpStatusCode.OK, getBalanceResponse);
            }
            catch (MessageSecurityException e)
            {
                Fault fault = new Fault(new APIBanking.Fault(e));

                HttpError myCustomError = new HttpError(fault.Message);

                c.writelog(e.Message, "MessageSecurityException", DateTime.Now, "", "");
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, myCustomError);
            }


            catch (TimeoutException ex)
            {
                HttpError myCustomError = new HttpError();
                myCustomError.Add("ErrorCode", 500);
                myCustomError.Add("Errormsg", ex.Message);
                myCustomError.Add("Ihno", AccountNumber);
                StringWriter sw = new StringWriter();
                XmlTextWriter tw = null;
                XmlSerializer serializer = new XmlSerializer(myCustomError.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, myCustomError);
                string tes = sw.ToString();
                c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), tes);
                //c.InsertResponse("500", ex.Message, requestReferenceNo, "");
                c.writelog(ex.Message, "TimeoutException", DateTime.Now, "", "");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, myCustomError);
            }
            catch (FaultException ex)
            {
                String faultCode = ex.Code.SubCode.Name;
                String FaultReason = ex.Message;

                message = faultCode + " - " + FaultReason;

                HttpError myCustomError = new HttpError();
                myCustomError.Add("ErrorCode", faultCode);
                myCustomError.Add("Errormsg", FaultReason);
                myCustomError.Add("Ihno", AccountNumber);
                StringWriter sw = new StringWriter();
                XmlTextWriter tw = null;
                XmlSerializer serializer = new XmlSerializer(myCustomError.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, myCustomError);
                string tes = sw.ToString();
                c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), tes);
                //c.InsertResponse(faultCode, FaultReason, requestReferenceNo, "");
                c.writelog(ex.Message, "FaultException", DateTime.Now, "", "");
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, myCustomError);
            }

            catch (CommunicationException ex)
            {
                HttpError myCustomError = new HttpError();
                myCustomError.Add("ErrorCode", 500);
                myCustomError.Add("Errormsg", ex.Message);
                myCustomError.Add("Ihno", AccountNumber);
                StringWriter sw = new StringWriter();
                XmlTextWriter tw = null;
                XmlSerializer serializer = new XmlSerializer(myCustomError.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, myCustomError);
                string tes = sw.ToString();
                c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), tes);
                c.writelog(ex.Message, "CommunicationException", DateTime.Now, "", "");
                //c.InsertResponse("500", ex.Message, requestReferenceNo, "");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, myCustomError);
            }

            catch (Exception ex)
            {
                HttpError myCustomError = new HttpError();
                myCustomError.Add("ErrorCode", 500);
                myCustomError.Add("Errormsg", "InternerlServer Error");
                myCustomError.Add("Ihno", AccountNumber);
                StringWriter sw = new StringWriter();
                XmlTextWriter tw = null;
                XmlSerializer serializer = new XmlSerializer(myCustomError.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, myCustomError);
                string tes = sw.ToString();
                c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), tes);
                //c.InsertResponse("500", ex.Message, requestReferenceNo, "");
                c.writelog(ex.Message, "Exception", DateTime.Now, "", "");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, myCustomError);
            }
            //return getBalanceResponse;
        }

    }

}
