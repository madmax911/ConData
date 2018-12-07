var PortNum = 8888;

var Http       = require('http');
var Child_Proc = require('child_process');

Http.createServer( function(Request, Response) {

    Request.on( 'data', function(Received_Query) {

        var Send_Json = '';
        var Mdb_And_Qry = Received_Query.toString("utf8").split("\r\n //// \r\n");
        console.log("\r\n\r\nNow executing query / Date: "
                    + new Date().toLocaleString('en-US', { hour12: true }).split('GMT')[0]
                    + "...  \r\n");
        console.log(Received_Query.toString("utf8"));

        var ConData = Child_Proc.spawn( 'ConData.exe', [Mdb_And_Qry[0]] );

        ConData.stdin.setEncoding('utf8');
        ConData.stdin.write(Mdb_And_Qry[1]);
        ConData.stdin.end();

        Response.statusCode = 200;
        Response.setHeader('Content-Type',      'application/json');
        Response.setHeader('Encoding-Encoding', 'chunked');
        Response.setHeader('Connection',        'close');
        Response.setHeader('Access-Control-Allow-Origin', '*');

        ConData.stdout.pipe(Response);
    });

}).listen(PortNum);