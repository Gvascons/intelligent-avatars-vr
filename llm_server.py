
from flask import Flask, request, jsonify, send_from_directory
import subprocess
import pyttsx3
import os
import whisper
import time
import soundfile as sf
import traceback
import numpy as np

app = Flask(__name__)

# Modelo de transcrição
whisper_model = whisper.load_model("base")

# TTS engine
engine = pyttsx3.init()

# Caminho de saída para o áudio da resposta
audio_filename = "response.wav"
audio_directory = os.path.join(os.getcwd(), 'static')
audio_path = os.path.join(audio_directory, audio_filename)
if not os.path.exists(audio_directory):
    os.makedirs(audio_directory)

# Prompt da personalidade
personality_prompt = """
Você é Marie Curie, uma cientista renomada, conhecida mundialmente por suas contribuições à ciência.
Responda de forma sábia, gentil, didática e breve como se estivesse explicando para um curioso interessado em ciência.
"""

@app.route('/speech-to-text-and-respond', methods=['POST'])
def speech_to_text_and_respond():
    if 'file' not in request.files:
        return jsonify({"error": "Arquivo de áudio não encontrado."}), 400

    audio_file = request.files['file']
    audio_path_input = "user_input.wav"
    audio_file.save(audio_path_input)

    # Aguarda o arquivo estar pronto
    for i in range(10):
        if os.path.exists(audio_path_input) and os.path.getsize(audio_path_input) > 1000:
            break
        time.sleep(0.1)
    else:
        return jsonify({"error": "Arquivo de áudio não salvo corretamente."}), 500

    # Verifica se o áudio tem conteúdo válido
    try:
        waveform, sr = sf.read(audio_path_input)
        if isinstance(waveform, np.ndarray) and waveform.size == 0:
            return jsonify({"error": "Áudio vazio ou corrompido."}), 400
    except Exception as e:
        traceback.print_exc()
        return jsonify({"error": f"Erro ao carregar o áudio: {str(e)}"}), 500

    # Transcrição com Whisper
    try:
        result = whisper_model.transcribe(audio_path_input)
        user_text = result['text']
        print("Usuário disse:", user_text)
    except Exception as e:
        traceback.print_exc()
        return jsonify({"error": f"Erro na transcrição: {str(e)}"}), 500

    # Gera resposta com modelo de linguagem
    response = ask_internal({"prompt": user_text})

    return jsonify({
        "transcription": user_text,
        "response": response["response"],
        "audio_path": response["audio_path"]
    })

def ask_internal(data):
    user_prompt = data.get('prompt')
    full_prompt = f"{personality_prompt}\nUsuário: {user_prompt}\nAssistente:"

    try:
        result = subprocess.run(
            ["ollama", "run", "llama3"],
            input=full_prompt.encode(),
            capture_output=True,
            check=True
        )
        response = result.stdout.decode().strip()
    except FileNotFoundError:
        response = "[ERRO] Ollama não está instalado ou não está no PATH."

    engine.save_to_file(response, audio_path)
    engine.runAndWait()

    return {
        "response": response,
        "audio_path": f"/static/{audio_filename}"
    }

@app.route('/static/<filename>')
def send_audio(filename):
    return send_from_directory(audio_directory, filename)

if __name__ == '__main__':
    app.run(port=5000)
