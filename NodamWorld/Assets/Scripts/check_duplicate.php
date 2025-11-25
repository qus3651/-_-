<?php
// ========== check_duplicate.php ==========
// 아이디 중복 확인 API

$DB_HOST = "localhost";
$DB_USER = "nodamcoin";
$DB_PASS = "ansdustn1!";
$DB_NAME = "nodamcoin";

header('Content-Type: application/json; charset=UTF-8');

$userid = isset($_POST['userid']) ? trim($_POST['userid']) : '';

if ($userid === '') {
    echo json_encode(['available' => false]);
    exit;
}

$conn = new mysqli($DB_HOST, $DB_USER, $DB_PASS, $DB_NAME);
if ($conn->connect_error) {
    echo json_encode(['available' => false]);
    exit;
}
$conn->set_charset("utf8mb4");

// 중복 체크
$stmt = $conn->prepare("SELECT userid FROM users WHERE userid = ?");
$stmt->bind_param("s", $userid);
$stmt->execute();
$result = $stmt->get_result();

$available = ($result->num_rows === 0);

$stmt->close();
$conn->close();

echo json_encode(['available' => $available]);
?>