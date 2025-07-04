document.addEventListener('DOMContentLoaded', function () {
    // Elementos do DOM
    const chatContainer = document.getElementById('chat-container');
    const floatingBtn = document.getElementById('floating-btn');
    const closeBtn = document.getElementById('close-btn');
    const notificationBadge = document.getElementById('notification-badge');
    const messagesContainer = document.getElementById('messages-container');
    const micBtn = document.getElementById('mic-btn');
    const recordingStatus = document.getElementById('recording-status');
    const recordingTimer = document.getElementById('recording-timer');
    const recordingProgress = document.getElementById('recording-progress');
    const progressFill = document.getElementById('progress-fill');
    const minTimeMessage = document.getElementById('min-time-message');

    // Estado do chat
    let isChatOpen = false;
    let hasUnreadMessages = false;
    let mediaRecorder;
    let audioChunks = [];
    let recordingStartTime;
    let timerInterval;
    let audioContext;

    // Abre/fecha o chat
    function toggleChat() {
        isChatOpen = !isChatOpen;

        if (isChatOpen) {
            chatContainer.classList.add('open');
            hasUnreadMessages = false;
            notificationBadge.style.display = 'none';
            setTimeout(() => {
                messagesContainer.scrollTop = messagesContainer.scrollHeight;
            }, 100);
        } else {
            chatContainer.classList.remove('open');
        }
    }

    // Adiciona mensagem ao chat com texto e áudio
    function addMessage(text, isReceived, audioData = null) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${isReceived ? 'message-received' : 'message-sent'}`;

        const now = new Date();
        const timeString = `${now.getHours()}:${now.getMinutes().toString().padStart(2, '0')}`;

        // Cria o conteúdo da mensagem
        let messageContent = `
            <div class="message-text">${text}</div>
            <div class="message-time">${timeString}</div>
        `;

        // Adiciona player de áudio se houver dados de áudio
        if (audioData && isReceived) {
            messageContent += `
                <div class="audio-player">
                    <button class="play-audio-btn">
                        <span class="material-symbols-outlined play-icon">play_circle</span>
                        <span class="material-symbols-outlined pause-icon" style="display:none;">pause</span>
                    </button>
                    <div class="audio-progress-container">
                        <div class="audio-progress-bar">
                            <div class="audio-progress"></div>
                            <div class="audio-progress-handle"></div>
                        </div>
                        <span class="audio-duration">0:00</span>
                    </div>
                </div>
            `;
        }

        messageDiv.innerHTML = messageContent;
        messagesContainer.appendChild(messageDiv);
        messagesContainer.scrollTop = messagesContainer.scrollHeight;

        // Inicializa o player de áudio se existir
        if (audioData && isReceived) {
            initAudioPlayer(messageDiv, audioData);
        }

        if (!isChatOpen && isReceived) {
            hasUnreadMessages = true;
            notificationBadge.style.display = 'flex';
            const currentCount = parseInt(notificationBadge.textContent) || 0;
            notificationBadge.textContent = currentCount + 1;
        }
    }

    // Inicializa o player de áudio
    function initAudioPlayer(messageDiv, audioData) {
        const playBtn = messageDiv.querySelector('.play-audio-btn');
        const playIcon = messageDiv.querySelector('.play-icon');
        const pauseIcon = messageDiv.querySelector('.pause-icon');
        const progressBar = messageDiv.querySelector('.audio-progress');
        const progressHandle = messageDiv.querySelector('.audio-progress-handle');
        const durationDisplay = messageDiv.querySelector('.audio-duration');
        const progressContainer = messageDiv.querySelector('.audio-progress-container');

        let audioBuffer;
        let audioSource;
        let isPlaying = false;
        let startOffset = 0;
        let startTime = 0;
        let duration = 0;
        let animationFrameId;

        // Inicializa o contexto de áudio se necessário
        if (!audioContext) {
            audioContext = new (window.AudioContext || window.webkitAudioContext)();
        }

        // Carrega o áudio e mostra a duração
        audioContext.decodeAudioData(audioData.buffer).then(buffer => {
            audioBuffer = buffer;
            duration = buffer.duration;

            // Formata a duração (mm:ss)
            const minutes = Math.floor(duration / 60);
            const seconds = Math.floor(duration % 60);
            durationDisplay.textContent = `0:00 / ${minutes}:${seconds.toString().padStart(2, '0')}`;
        });

        // Função para atualizar a barra de progresso
        function updateProgress() {
            if (!audioBuffer) return;

            const currentTime = audioContext.currentTime - startTime + startOffset;
            const progressPercent = (currentTime / duration) * 100;

            progressBar.style.width = `${progressPercent}%`;
            progressHandle.style.left = `${progressPercent}%`;

            // Atualiza o tempo decorrido
            const currentMinutes = Math.floor(currentTime / 60);
            const currentSeconds = Math.floor(currentTime % 60);
            const totalMinutes = Math.floor(duration / 60);
            const totalSeconds = Math.floor(duration % 60);

            durationDisplay.textContent = `${currentMinutes}:${currentSeconds.toString().padStart(2, '0')} / ${totalMinutes}:${totalSeconds.toString().padStart(2, '0')}`;

            if (isPlaying && currentTime < duration) {
                animationFrameId = requestAnimationFrame(updateProgress);
            } else if (currentTime >= duration) {
                stopAudio();
            }
        }

        // Função para tocar o áudio
        function playAudio() {
            if (!audioBuffer) return;

            if (!isPlaying) {
                isPlaying = true;
                playIcon.style.display = 'none';
                pauseIcon.style.display = 'inline';

                // Cria uma nova fonte de áudio
                audioSource = audioContext.createBufferSource();
                audioSource.buffer = audioBuffer;
                audioSource.connect(audioContext.destination);

                // Define o ponto de início corretamente
                startTime = audioContext.currentTime;
                audioSource.start(0, startOffset % duration); // Usa módulo para evitar valores além da duração

                audioSource.onended = () => {
                    stopAudio();
                };

                updateProgress();
            }
        }

        // Função para pausar o áudio
        function pauseAudio() {
            if (isPlaying && audioSource) {
                isPlaying = false;
                playIcon.style.display = 'inline';
                pauseIcon.style.display = 'none';

                // Calcula o tempo decorrido corretamente
                const elapsed = audioContext.currentTime - startTime;
                startOffset += elapsed;

                // Para a fonte de áudio
                audioSource.stop();
                cancelAnimationFrame(animationFrameId);
            }
        }

        // Função para parar o áudio
        function stopAudio() {
            isPlaying = false;
            playIcon.style.display = 'inline';
            pauseIcon.style.display = 'none';

            if (audioSource) {
                audioSource.stop();
                audioSource = null;
            }

            cancelAnimationFrame(animationFrameId);
            startOffset = 0; // Isso zera a posição quando o áudio termina
            progressBar.style.width = '0%';
            progressHandle.style.left = '0%';

            if (audioBuffer) {
                const totalMinutes = Math.floor(audioBuffer.duration / 60);
                const totalSeconds = Math.floor(audioBuffer.duration % 60);
                durationDisplay.textContent = `0:00 / ${totalMinutes}:${totalSeconds.toString().padStart(2, '0')}`;
            }
        }

        // Função para buscar um ponto específico no áudio
        function seekAudio(e) {
            if (!audioBuffer) return;

            const rect = progressContainer.getBoundingClientRect();
            const seekPercent = Math.min(1, Math.max(0, (e.clientX - rect.left) / rect.width));
            startOffset = Math.min(duration, seekPercent * duration); // Garante que não ultrapasse a duração

            if (isPlaying) {
                pauseAudio();
                playAudio();
            } else {
                progressBar.style.width = `${(startOffset / duration) * 100}%`;
                progressHandle.style.left = `${(startOffset / duration) * 100}%`;

                const currentMinutes = Math.floor(startOffset / 60);
                const currentSeconds = Math.floor(startOffset % 60);
                const totalMinutes = Math.floor(duration / 60);
                const totalSeconds = Math.floor(duration % 60);
                durationDisplay.textContent = `${currentMinutes}:${currentSeconds.toString().padStart(2, '0')} / ${totalMinutes}:${totalSeconds.toString().padStart(2, '0')}`;
            }
        }

        // Event listeners
        playBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            if (isPlaying) {
                pauseAudio();
            } else {
                playAudio();
            }
        });

        progressContainer.addEventListener('click', function (e) {
            seekAudio(e);
        });

        // Adiciona arrastar para a bolinha de progresso
        progressHandle.addEventListener('mousedown', function (e) {
            e.stopPropagation();
            e.preventDefault();

            const handleMove = function (e) {
                seekAudio(e);
            };

            const handleUp = function () {
                document.removeEventListener('mousemove', handleMove);
                document.removeEventListener('mouseup', handleUp);
            };

            document.addEventListener('mousemove', handleMove);
            document.addEventListener('mouseup', handleUp);
        });
    }

    // Inicia a gravação de áudio
    async function startRecording() {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({
                audio: {
                    echoCancellation: true,
                    noiseSuppression: true,
                    channelCount: 1
                }
            });

            mediaRecorder = new MediaRecorder(stream, {
                mimeType: 'audio/webm',
                audioBitsPerSecond: 128000
            });

            audioChunks = [];

            mediaRecorder.ondataavailable = event => {
                if (event.data.size > 0) {
                    audioChunks.push(event.data);
                }
            };

            mediaRecorder.onstop = async () => {
                const elapsedTime = Math.floor((Date.now() - recordingStartTime) / 1000);
                if (elapsedTime < 2) {
                    minTimeMessage.style.display = 'block';
                    return;
                }

                const audioBlob = new Blob(audioChunks, { type: 'audio/webm' });
                await sendAudioToServer(audioBlob);
                stream.getTracks().forEach(track => track.stop());
            };

            mediaRecorder.start(100); // Coleta dados em chunks de 100ms
            recordingStartTime = Date.now();
            updateTimer();
            timerInterval = setInterval(updateTimer, 1000);

            micBtn.classList.add('recording');
            recordingStatus.style.display = 'flex';
            recordingProgress.style.display = 'block';
            minTimeMessage.style.display = 'none';
        } catch (error) {
            console.error('Erro ao acessar microfone:', error);
            addMessage("Não foi possível acessar o microfone. Por favor, verifique as permissões.", true);
        }
    }

    // Para a gravação de áudio
    function stopRecording() {
        if (mediaRecorder && mediaRecorder.state !== 'inactive') {
            mediaRecorder.stop();
            clearInterval(timerInterval);

            micBtn.classList.remove('recording');
            recordingStatus.style.display = 'none';
            recordingProgress.style.display = 'none';
        }
    }

    // Atualiza o timer da gravação
    function updateTimer() {
        const elapsedTime = Math.floor((Date.now() - recordingStartTime) / 1000);
        const minutes = Math.floor(elapsedTime / 60);
        const seconds = elapsedTime % 60;
        recordingTimer.textContent = `${minutes}:${seconds.toString().padStart(2, '0')}`;

        const progressPercent = Math.min(100, (elapsedTime / 30) * 100);
        progressFill.style.width = `${progressPercent}%`;

        // Para automaticamente após 30 segundos
        if (elapsedTime >= 30) {
            stopRecording();
        }
    }

    // Envia o áudio para o servidor
    async function sendAudioToServer(audioBlob) {
        const formData = new FormData();
        formData.append('audioFile', audioBlob, 'recording.webm');

        try {
            addMessage("Processando sua mensagem...", false);

            const response = await fetch('/api/audio/process', {
                method: 'POST',
                body: formData,
                headers: {
                    'Accept': 'application/json' // Especifica que queremos JSON de volta
                }
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Erro no servidor: ${errorText}`);
            }

            // Verifica se a resposta é JSON
            const contentType = response.headers.get('content-type');
            if (!contentType || !contentType.includes('application/json')) {
                const responseData = await response.text();
                console.error('Resposta não é JSON:', responseData);
                throw new Error('Resposta do servidor não está no formato esperado');
            }

            const result = await response.json();

            if (result.status === 'unauthenticated') {
                addMessage("Você precisa estar logado para usar este recurso.", true);
                return;
            }

            // Processa a resposta de acordo com a estrutura esperada
            let messageText, audioBytes;

            if (result.message && result.audioData) {
                messageText = result.message;
                audioBytes = base64ToUint8Array(result.audioData);
            } else if (result.Message && result.AudioData) {
                messageText = result.Message;
                audioBytes = base64ToUint8Array(result.AudioData);
            } else if (result.AudioData && result.Message) {
                // Outra variação possível de nomenclatura
                messageText = result.Message;
                audioBytes = base64ToUint8Array(result.AudioData);
            } else {
                console.error('Formato de resposta inesperado:', result);
                throw new Error('Formato de resposta inesperado do servidor');
            }

            addMessage(messageText, true, audioBytes);

        } catch (error) {
            console.error('Erro ao enviar áudio:', error);
            addMessage("Ocorreu um erro ao processar seu áudio. Por favor, tente novamente.", true);
        }
    }

    // Função auxiliar para converter base64 para Uint8Array
    function base64ToUint8Array(base64) {
        const binaryString = atob(base64);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        return bytes;
    }

    // Carrega a mensagem de boas-vindas
    async function loadWelcomeMessage() {
        try {
            const response = await fetch('/api/audio/welcome');
            if (response.ok) {
                const data = await response.json();
                if (data.text && data.audioData) {
                    // Converte o base64 para Uint8Array
                    const binaryString = atob(data.audioData);
                    const bytes = new Uint8Array(binaryString.length);
                    for (let i = 0; i < binaryString.length; i++) {
                        bytes[i] = binaryString.charCodeAt(i);
                    }

                    addMessage(data.text, true, bytes);
                }
            }
        } catch (error) {
            console.error('Erro ao carregar mensagem de boas-vindas:', error);
        }
    }

    // Event listeners
    floatingBtn.addEventListener('click', toggleChat);
    closeBtn.addEventListener('click', toggleChat);

    // Botão de microfone
    micBtn.addEventListener('mousedown', startRecording);
    micBtn.addEventListener('touchstart', startRecording);
    micBtn.addEventListener('mouseup', stopRecording);
    micBtn.addEventListener('touchend', stopRecording);
    micBtn.addEventListener('mouseleave', stopRecording);

    // Carrega mensagem inicial
    loadWelcomeMessage();
});