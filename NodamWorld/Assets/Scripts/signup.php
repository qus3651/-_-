<?php
// ========== signup.php ==========
// 회원가입 처리 API

$DB_HOST = "localhost";
$DB_USER = "nodamcoin";
$DB_PASS = "ansdustn1!";
$DB_NAME = "nodamcoin";

header('Content-Type: application/json; charset=UTF-8');

// 입력 값 받기
$userid = isset($_POST['userid']) ? trim($_POST['userid']) : '';
$password = isset($_POST['password']) ? trim($_POST['password']) : '';
$name = isset($_POST['name']) ? trim($_POST['name']) : '';
$wallet = isset($_POST['wallet']) ? trim($_POST['wallet']) : '';
$phone = isset($_POST['phone']) ? trim($_POST['phone']) : '';

// 필수 항목 체크
if ($userid === '' || $password === '' || $name === '') {
    echo json_encode(['ok' => false, 'error' => '필수 항목을 입력해주세요.']);
    exit;
}

// 아이디 길이 체크
if (strlen($userid) < 4) {
    echo json_encode(['ok' => false, 'error' => '아이디는 4자 이상이어야 합니다.']);
    exit;
}

// 비밀번호 길이 체크
if (strlen($password) < 4) {
    echo json_encode(['ok' => false, 'error' => '비밀번호는 4자 이상이어야 합니다.']);
    exit;
}

$conn = new mysqli($DB_HOST, $DB_USER, $DB_PASS, $DB_NAME);
if ($conn->connect_error) {
    echo json_encode(['ok' => false, 'error' => 'DB 연결 실패']);
    exit;
}
$conn->set_charset("utf8mb4");

// 중복 체크 한 번 더
$check_stmt = $conn->prepare("SELECT userid FROM users WHERE userid = ?");
$check_stmt->bind_param("s", $userid);
$check_stmt->execute();
$check_result = $check_stmt->get_result();

if ($check_result->num_rows > 0) {
    $check_stmt->close();
    $conn->close();
    echo json_encode(['ok' => false, 'error' => '이미 존재하는 아이디입니다.']);
    exit;
}
$check_stmt->close();

// 회원가입 처리
$stmt = $conn->prepare("INSERT INTO users (userid, password, name, wallet, phone, created_at, gamemoney) VALUES (?, ?, ?, ?, ?, NOW(), 0)");
$stmt->bind_param("sssss", $userid, $password, $name, $wallet, $phone);

if ($stmt->execute()) {
    echo json_encode(['ok' => true]);
} else {
    echo json_encode(['ok' => false, 'error' => '회원가입 처리 중 오류가 발생했습니다.']);
}

$stmt->close();
$conn->close();
?>