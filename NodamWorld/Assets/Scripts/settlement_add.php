<?php
// settlement_add.php
$DB_HOST = "localhost";
$DB_USER = "nodamcoin";
$DB_PASS = "ansdustn1!";
$DB_NAME = "nodamcoin";

header('Content-Type: application/json; charset=UTF-8');

$userid   = isset($_POST['userid']) ? trim($_POST['userid']) : '';
$name     = isset($_POST['name']) ? trim($_POST['name']) : '';
$wallet   = isset($_POST['wallet']) ? trim($_POST['wallet']) : '';
$amount   = isset($_POST['amount']) ? intval($_POST['amount']) : 0;

if ($userid === '' || $wallet === '' || $amount <= 0) {
    echo json_encode(['ok' => false, 'error' => 'INVALID_INPUT']);
    exit;
}

$conn = new mysqli($DB_HOST, $DB_USER, $DB_PASS, $DB_NAME);
if ($conn->connect_error) {
    echo json_encode(['ok' => false, 'error' => 'DB_CONNECT_FAIL']);
    exit;
}
$conn->set_charset("utf8mb4");

// 이미 존재하는 row 확인
$stmt = $conn->prepare("SELECT id, amount FROM settlement WHERE userid=? AND wallet=?");
$stmt->bind_param("ss", $userid, $wallet);
$stmt->execute();
$res = $stmt->get_result();

if ($row = $res->fetch_assoc()) {
    $newAmount = $row['amount'] + $amount;
    $upd = $conn->prepare("UPDATE settlement SET amount=? WHERE id=?");
    $upd->bind_param("ii", $newAmount, $row['id']);
    $ok = $upd->execute();
    $upd->close();
} else {
    $ins = $conn->prepare("INSERT INTO settlement (userid, name, wallet, amount) VALUES (?,?,?,?)");
    $ins->bind_param("sssi", $userid, $name, $wallet, $amount);
    $ok = $ins->execute();
    $ins->close();
}
$stmt->close();
$conn->close();

echo json_encode(['ok' => $ok]);
