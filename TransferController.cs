using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using UTI_InstaRedemption.com;

namespace UTI_InstaRedemption.Controllers
{
    public class TransferController : ApiController
    {

        Common c = new Common();
        string message = "";
        [HttpPost]
        public HttpResponseMessage TransferBal(string beneficiaryAccountNo, string beneficiaryIFSC, string beneficiaryMMID, string beneficiaryMobileNo, string Name, string address1,
                string emailID, string mobileNo, string uniqueRequestNo, string appID, string customerID, string debitAccountNo, float transferAmount)
        {
            DataSet ds = new DataSet();
            string URL = "Transfer/TransferBal&beneficiaryAccountNo?" + beneficiaryAccountNo + "&beneficiaryIFSC?" + beneficiaryIFSC + "&beneficiaryMMID?" + beneficiaryMMID + "&beneficiaryMobileNo?" + beneficiaryMobileNo + "&Name?" + Name + "&address1?" + address1 + "&emailID?" + emailID + "&mobileNo?" + mobileNo + "&uniqueRequestNo?" + uniqueRequestNo + "&appID?" + appID + "&customerID?" + customerID + "&debitAccountNo?" + debitAccountNo + "&transferAmount?" + transferAmount + "";
            ds = c.getInserlogrequest(URL);
            APIBanking.Environment env = new APIBanking.Environments.YBL.UAT("2449810", "Yesbank1", "7a7a26d8-1679-436b-854a-a2b5682bbf11", "nP8oE0tO5wR5kI1qD3aA6aR6wD6hR7hB8oP6qW5vU0hN0wE4sD", null);
           // APIBanking.Environment env = new APIBanking.Environments.YBL.UAT(ConfigurationManager.AppSettings["customerId"].ToString(), ConfigurationManager.AppSettings["Password"].ToString(), ConfigurationManager.AppSettings["clientId"].ToString(), ConfigurationManager.AppSettings["clientSecret"].ToString(), ConfigurationManager.AppSettings["CertificatePath"].ToString(), "123");
            com.transfer gTransfer = new transfer();
            com.transferRequest gTransferRequest = new transferRequest();
            com.transferResponse gTransferResponse = new transferResponse();



            beneficiaryDetailType b = new beneficiaryDetailType();
            b.beneficiaryAccountNo = beneficiaryAccountNo;
            b.beneficiaryIFSC = beneficiaryIFSC;
            b.beneficiaryMMID = beneficiaryMMID;
            b.beneficiaryMobileNo = beneficiaryMobileNo;

            beneficiaryType bt = new beneficiaryType();
            nameType nm = new nameType();
            nm.Item = Name;

            AddressType ad = new AddressType();
            ad.address1 = address1;
            //ad.address2 = "";
            // ad.address3 = "";
            // ad.city = "";
            ad.country = "IN";
            //ad.postalCode = "";

            contactType ct = new contactType();
            ct.emailID = emailID;
            ct.mobileNo = mobileNo;

            b.beneficiaryName = nm;
            b.beneficiaryAddress = ad;
            b.beneficiaryContact = ct;

            gTransfer.beneficiary = bt;
            gTransfer.beneficiary.Item = b;
            gTransfer.version = "2";
            gTransfer.uniqueRequestNo = uniqueRequestNo; //Ihno
            gTransfer.appID = appID;
            gTransfer.customerID = customerID;
            gTransfer.debitAccountNo = debitAccountNo;
            gTransfer.transferAmount = transferAmount;
            //gTransfer.transferType = transferTypeType.IMPS;
            gTransfer.transferType = transferTypeType.IMPS;
            gTransfer.transferCurrencyCode = currencyCodeType.INR;
            gTransfer.remitterToBeneficiaryInfo = "FUND TRANSFER";
            try
            {
                gTransferResponse = APIBanking.DomesticRemittanceClient.getTransfer(env, gTransfer);
                //return Request.CreateResponse(HttpStatusCode.OK, getBalanceResponse);
                StringWriter sw = new StringWriter();
                XmlTextWriter tw = null;
                XmlSerializer serializer = new XmlSerializer(gTransferResponse.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, gTransferResponse);
                string tes = sw.ToString();
                c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), tes.ToString());
                c.InsertResponse(gTransferResponse.transactionStatus.subStatusCode, gTransferResponse.transactionStatus.statusCode.ToString(), gTransferResponse.requestReferenceNo, gTransferResponse.transactionStatus.bankReferenceNo);
                return this.Request.CreateResponse(HttpStatusCode.OK, gTransferResponse);
            }
            catch (FaultException ex)
            {

                String faultCode = ex.Code.SubCode.Name;
                String FaultReason = ex.Message;
                message = faultCode + " - " + FaultReason;
                HttpError myCustomError = new HttpError();
                myCustomError.Add("ErrorCode", faultCode);
                myCustomError.Add("Errormsg", FaultReason);
                myCustomError.Add("Ihno", uniqueRequestNo);
                StringWriter sw = new StringWriter();
                XmlTextWriter tw = null;
                XmlSerializer serializer = new XmlSerializer(myCustomError.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, myCustomError);
                string tes = sw.ToString();
                c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), tes);
                c.InsertResponse(faultCode, FaultReason, uniqueRequestNo, "");
                c.writelog(ex.Message, "FaultException", DateTime.Now, "", "");
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed, myCustomError);
            }
            //catch (TimeoutException ex)
            //{
            //    message = ex.Message;
            //    HttpError myCustomError = new HttpError(message);
            //   // return Request.CreateErrorResponse(HttpStatusCode.BadRequest, myCustomError);
            //    //return this.Request.CreateResponse(HttpStatusCode.OK, gTransferResponse);
            //}
            //catch (CommunicationException ex)
            //{
            //    message = ex.Message;
            //    HttpError myCustomError = new HttpError(message);
            //   // return Request.CreateErrorResponse(HttpStatusCode.BadRequest, myCustomError);
            //    //return this.Request.CreateResponse(HttpStatusCode.OK, gTransferResponse);
            //}
            catch (Exception ex)
            {

                c.writelog(ex.Message, "TransferBal", DateTime.Now, "", "");
                HttpError myCustomError = new HttpError();
                myCustomError.Add("ErrorCode", 500);
                myCustomError.Add("Errormsg", "InternerlServer Error");
                myCustomError.Add("Ihno", uniqueRequestNo);
                StringWriter sw = new StringWriter();
                XmlTextWriter tw = null;
                XmlSerializer serializer = new XmlSerializer(myCustomError.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, myCustomError);
                string tes = sw.ToString();
                c.updatelogrequest(Convert.ToInt32(ds.Tables[0].Rows[0]["KMR_Slno"]), tes);
                c.InsertResponse("500", ex.Message, uniqueRequestNo, "");
                c.writelog(ex.Message, "TransferBal", DateTime.Now, "", "");
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, myCustomError);
            }
        }
    }
}
