<?php
// gamemoney_add.php
$DB_HOST = "localhost";
$DB_USER = "nodamcoin";
$DB_PASS = "ansdustn1!";
$DB_NAME = "nodamcoin";

header('Content-Type: application/json; charset=UTF-8');

$userid = isset($_POST['userid']) ? trim($_POST['userid']) : '';
$amount = isset($_POST['amount']) ? intval($_POST['amount']) : 0;

// amount가 0이 아니고 userid가 있으면 OK (음수도 허용)
if ($userid === '' || $amount == 0) {
    echo json_encode(['ok' => false, 'error' => 'INVALID_INPUT']);
    exit;
}

$conn = new mysqli($DB_HOST, $DB_USER, $DB_PASS, $DB_NAME);
if ($conn->connect_error) {
    echo json_encode(['ok' => false, 'error' => 'DB_CONNECT_FAIL']);
    exit;
}
$conn->set_charset("utf8mb4");

// 게임머니 업데이트
$stmt = $conn->prepare("UPDATE users SET gamemoney = gamemoney + ? WHERE userid=?");
$stmt->bind_param("is", $amount, $userid);
$ok = $stmt->execute();
$stmt->close();

// 최신 게임머니 값 불러오기
$newBalance = 0;
if ($ok) {
    $res = $conn->prepare("SELECT gamemoney FROM users WHERE userid=?");
    $res->bind_param("s", $userid);
    $res->execute();
    $res->bind_result($newBalance);
    $res->fetch();
    $res->close();
}

$conn->close();

echo json_encode(['ok' => $ok, 'gamemoney' => intval($newBalance)]);
?>