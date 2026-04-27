import React, { useState, useEffect, useContext, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { ApiHelper } from '../../dvt_api/ApiHelper';
import { DvtApiPaths } from '../../dvt_api/DvtApiPaths';
import axios from 'axios';

function StatusProgress(props) {
    const [progress, setProgress] = useState(null);
    const [connected, setConnected] = useState(false);
    const [paused, setPaused] = useState(false);
    const pausedRef = useRef(false);
    const connectionRef = useRef(null);
    const { jobId, statusProgressData, setStatusProgressData } = props;

    useEffect(() => {
        // Optionally auto-connect on mount or when jobId changes
        if (jobId) {
            console.log(`Auto-connecting for jobId: ${jobId}`);
            connect();
        }
        return () => {
            disconnect();
        };
    }, [jobId]);

    // Connect handler
    const connect = async () => {
        const utcNow = new Date().toISOString();
        console.log(`Setting up SignalR connection at UTC: ${utcNow}`);
        // Step 1: Call REST API to register job status
        const registerResponse = await axios.get(ApiHelper.getApiUrlWithId(DvtApiPaths.Home.GetJobStatus, jobId));

        if (!registerResponse.status || registerResponse.status !== 200) {
            throw new Error(`Failed to register job status: ${registerResponse.status}`);
        }

        console.log('Job registered via REST API');
        try {
            if (!connectionRef.current) {
                const connection = new signalR.HubConnectionBuilder()
                    .withUrl(ApiHelper.getApiUrl(DvtApiPaths.Home.GetStatusProgress), {
                        accessTokenFactory: () => ApiHelper.getBearerToken(),
                        skipNegotiation: true, // <--- set to true here
                        transport: signalR.HttpTransportType.WebSockets
                    })
                    .withAutomaticReconnect()
                    .configureLogging(signalR.LogLevel.Information)
                    .build();

                connection.on("ReceiveJobStatusUpdate", (update) => {
                    console.log("Job status update received:", update);
                    setStatusProgressData(update);
                });

                connection.on("ReceiveValidationStatusUpdate", (update) => {
                    console.log("Validation status update received:", update);
                });

                connection.onclose((error) => {
                    console.error('Connection closed:', error);
                    setConnected(false);
                });

                connection.onreconnecting((error) => {
                    console.warn('Reconnecting...', error);
                });

                connection.onreconnected((connectionId) => {
                    console.log('Reconnected. ConnectionId:', connectionId);
                });

                await connection.start()
                    .then(() => {
                        console.log('SignalR Connected successfully');
                        connection.invoke("RegisterJobWithClient", jobId)
                            .then(() => console.log("Registered for job group:", jobId))
                            .catch(err => console.error("RegisterJobWithClient error:", err));
                        setConnected(true);
                    })
                    .catch(err => {
                        console.error("SignalR Connection Error: ", err);
                    });

                connectionRef.current = connection;
            }
        } catch (error) {
            console.error('Error connecting to SignalR:', error);
            if (error && error.message) {
                console.error('Error details:', error.message);
            }
        }
    };

    // Disconnect handler
    const disconnect = async () => {
        if (connectionRef.current) {
            await connectionRef.current.stop();
            connectionRef.current = null;
            setConnected(false);
            setProgress(null);
        }
    };

    // Pause handler
    const pause = () => {
        setPaused(true);
        pausedRef.current = true;
    };

    // Resume handler
    const resume = () => {
        setPaused(false);
        pausedRef.current = false;
    };

    // Button handler to invoke SendTestProgress
    const handleSendTestProgress = async () => {
        if (connectionRef.current) {
            try {
                await connectionRef.current.invoke('SendTestProgress');
            } catch (err) {
                console.error('Error invoking SendTestProgress:', err);
            }
        }
    };

    return (
        <div className='statusProgressContainer'>
            {/* <button onClick={connect} disabled={connected}>Connect</button>
            <button onClick={disconnect} disabled={!connected}>Disconnect</button>
            <button onClick={pause} disabled={paused || !connected}>Pause</button>
            <button onClick={resume} disabled={!paused || !connected}>Resume</button>
            <button onClick={handleSendTestProgress}>Send Test Progress</button>
            {progress ? (
                <div>
                    <p>Progress: {progress.percentage}%</p>
                    <p>Status: {progress.status}</p>
                    <p>Message: {progress.message}</p>
                </div>
            ) : (
                <p>No progress yet.</p>
            )} */}
        </div>
    );
}

export default StatusProgress;