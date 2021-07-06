var conn;
var peer = new Peer({ key: 'YOUR_API_KEY', debug: 3});
 
peer.on('open', function(){
    $('#my-id').text(peer.id);
});
 
peer.on('connection', function(connection){
    conn = connection;
 
    conn.on("open", function() {
        $("#peer-id").text(conn.id);
    });
 
    conn.on("data", onRecvMessage);
});
 
function onRecvMessage(data) {
    // byteで来るので文字列に変換
    var message = new TextDecoder().decode(data)
    $("#messages").append($("<p>").text(conn.id + ": " + message).css("font-weight", "bold"));
}
 
$(function() {
    $("#connect").click(function() {
        var peer_id = $('#peer-id-input').val();
        conn = peer.connect(peer_id);
        conn.on("open", function() {
            $("#peer-id").text(conn.id);
        });
 
        conn.on("data", onRecvMessage);
    });

    $("#send").click(function() {
        var message = $("#message").val();
        conn.send(message);
        $("#messages").append($("<p>").html(peer.id + ": " + message));
        $("#message").val("");
    });

    $("#close").click(function() {
        conn.close();
    });
});