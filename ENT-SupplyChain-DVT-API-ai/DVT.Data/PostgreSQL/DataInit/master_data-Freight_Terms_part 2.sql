--Bug 20072640: [QA Bug] - some Freight term is not yet in the db
--The data for Freight Terms in master_data table
--Data from this excel: https://emerson.sharepoint.com/:x:/r/sites/GPSTeam/_layouts/15/Doc.aspx?sourcedoc=%7BBBB56495-DF6E-4A23-BF33-BD4F07B6C8CB%7D&file=DVT%20Database%20Reference%20V2.2.xlsx&action=default&mobileredirect=true
--The data is a part of Freight Terms, from Sheet name: Freight Terms - Domestic Freight Terms.
--Domestic Freight Terms
INSERT INTO master_data VALUES('b71abbcc-c696-44d9-8809-075945984139','FreightTerms','UCC FOB ORIGIN PPD','','','','','','','','', now(),'bob-bw.wang@emerson.com', false);
INSERT INTO master_data VALUES('f518d9f7-776b-4807-a927-c4ca25c0e3db','FreightTerms','UCC FOB ORIGIN COL','','','','','','','','', now(),'bob-bw.wang@emerson.com', false);
INSERT INTO master_data VALUES('4314515d-7fe8-4285-8268-6373cd9e21fd','FreightTerms','UCC FOB DEST PPD','','','','','','','','', now(),'bob-bw.wang@emerson.com', false);
INSERT INTO master_data VALUES('7ca0433f-86ef-44dc-a860-f61bcdb65486','FreightTerms','UCC FOB DEST COL','','','','','','','','', now(),'bob-bw.wang@emerson.com', false);
INSERT INTO master_data VALUES('64dbe233-750e-4f65-99a3-2734e9b56c7d','FreightTerms','NPD','','','','','','','','', now(),'bob-bw.wang@emerson.com', false);