# ConData
RESTful SQL/Access query to JSON system

This is a simple DB server that will allow you to send Queries as POST requests and receive back a table in JSON format with added metadata.

It's quite handy as a lightweight rapid testing and development system.

To start it you can create the folder C:\Data and drop the files in there.

Then run the Start_server.bat script to run the back-end part.  This will require NodeJS.

Once it starts, you can open the example provided (PurchasesPastDue_example.html).

#--------------------------------------------------------------------------------

The perfect example:

```JavaScript
// ADO over http...

let uri   = "http://MyServer";
let query = "SELECT * FROM Alloys";

let data = await fetch(uri, query).then(res => res.json());

console.log(data);

// You get this in the http response:

{
  "ErrNum":      0,
  "ErrDesc":     "",
  "StartTime":   "11/26/2022 11:59:25 AM",
  "FieldCount":  3,
  "FieldIDs":    { "Alloy": 0, "BasePrice": 1, "GLCode": 2 },
  "SubTypes":    [ "String",   "Decimal",      "String"    ],
  "Types":       [ "string",   "number",       "string"    ],
  "Names":       [ "Alloy",    "BasePrice",    "GLCode"    ],
  
  "Values":
  [
                 [ "Aluminum",  309.382,       "4204"      ],
                 [ "Brass",     562.246,       "4203"      ],
                 [ "Stainless", 380.6,         "4206"      ],
                 [ "Steel",     372.55,        "4205"      ]
  ],

  "RecordCount": 4,
  "EndTime":     "11/26/2022 11:59:25 AM"
}

```
