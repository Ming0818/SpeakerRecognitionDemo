$(document).ready(function () {
    var recorder, $btn, intervalId;
    
    $(document).on(
        "click",
        ".start-rec",
        function () {
            $btn = $(this);

            var mediaConstraints = {
                video: false,
                audio: true
            };

            navigator.mediaDevices
                .getUserMedia(mediaConstraints)
                .then(onAudioStremAquired)
                .catch(errorCallback);
        });

    function onAudioStremAquired(audioStream) {
        // do actual work
        var options = {
            type: "audio",
            recorderType: StereoAudioRecorder,
            mimeType: 'audio/wav',
            sampleRate: 48000
        };



        recorder = RecordRTC(audioStream, options);
        recorder.startRecording();

        // do ui magic
        var $timerElem = $btn.parent().find(".info-block");
        var elapsedTime = 0;
        renderTime($timerElem, 0);
        intervalId = setInterval(function () {
                elapsedTime++;
                renderTime($timerElem, elapsedTime);
            },
            1000);
        $btn.text("Stop");
        $btn.toggleClass("start-rec").toggleClass("stop-rec").toggleClass("btn-info").toggleClass("btn-danger");
    }

    $(document).on(
        "click",
        ".stop-rec",
        function() {
            // do ui magic
            var $btn = $(this);
            
            var $elem = $btn.parent().find(".info-block");

            $btn.text("Rec");
            $btn.toggleClass("start-rec").toggleClass("stop-rec").toggleClass("btn-info").toggleClass("btn-danger");

            clearInterval(intervalId);
            $elem.text("");


            // do actual work
            recorder.stopRecording(function() {

                var fileType = 'audio';
                var fileName = 'recording.wav';

                var formData = new FormData();
                formData.append(fileType + '-filename', fileName);
                formData.append(fileType + '-blob', recorder.getBlob());

                var profileId = $btn.data("id");
                if (profileId) {
                    xhr('/enroll/' + profileId,
                        formData,
                        function (result) {
                            startMonitoringOperationStatus(result.operationId);
                        });
                } else {
                    xhr('/identify',
                        formData,
                        function (result) {
                            $elem.text(result.lookupResult);
                        });
                }
                
            });

        });

    function startMonitoringOperationStatus(operationId) {
        
    }

    function renderTime($elem, elapsedTime) {
        var secs = elapsedTime % 60;
        var mins = parseInt(elapsedTime / 60);
        var conj = secs < 10 ? ":0" : ":";
        $elem.text(mins.toString() + conj + secs.toString());
    }

    function errorCallback(error) {
        alert("error: " + error);
    }

    function xhr(url, data, callback) {
        var method;
        if (typeof (callback) === "function") {
            method = "POST";
        } else if (typeof (callback) === "undefined" && typeof (data) === "function") {
            method = "GET";
            callback = data;
        } else {
            throw new Error("can't resolve xhr method");
        }
    
        var request = new XMLHttpRequest();
        request.onreadystatechange = function () {
            if (request.readyState == 4 && request.status >= 200 && request.status < 300) {
                callback(JSON.parse(request.responseText));
            }
        };

        request.open(method, url);
        if (method === "GET")
            request.send();
        else
            request.send(data);
    }
});