mergeInto(LibraryManager.library, {
    // 토스 게임 로그인 - getUserKeyForGame 호출
    TossGetUserKeyForGame: function() {
        console.log('[TossGameLogin] Requesting user key for game...');
        
        if (window.tossInApp && window.tossInApp.getUserKeyForGame) {
            window.tossInApp.getUserKeyForGame()
                .then(function(result) {
                    console.log('[TossGameLogin] Success:', result);
                    // result = { hash: "abc123..." }
                    var hashValue = result.hash || '';
                    
                    // Unity로 해시 전달
                    SendMessage('TossGameLoginManager', 'OnGetUserKeySuccess', hashValue);
                })
                .catch(function(error) {
                    console.error('[TossGameLogin] Failed:', error);
                    // 실패 시 빈 문자열 전달 (테스트 모드)
                    SendMessage('TossGameLoginManager', 'OnGetUserKeyFailed', error.message || 'Failed to get user key');
                });
        } else {
            // 토스 인앱 환경이 아님 (개발/테스트 환경)
            console.warn('[TossGameLogin] Not in Toss InApp - using test mode');
            // 테스트 모드: 빈 문자열 전달
            SendMessage('TossGameLoginManager', 'OnGetUserKeySuccess', '');
        }
    },

    // 토스 인앱 환경 여부 확인
    IsTossInAppEnvironment: function() {
        return (window.tossInApp && window.tossInApp.getUserKeyForGame) ? 1 : 0;
    }
});