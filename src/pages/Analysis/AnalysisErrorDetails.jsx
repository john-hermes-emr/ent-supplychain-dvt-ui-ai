import React, { useState, useEffect } from 'react'
import style from './analysis.module.css'
import { ApiHelper } from '../../dvt_api/ApiHelper';
import { DvtApiPaths } from '../../dvt_api/DvtApiPaths';
import axios from 'axios';
import { convertUTCTimeToLocalTime } from '../../utils/js/utils.js';

const FullPageLoader = React.lazy(() => import('../../components/FullPageLoader/index.jsx'));


function AnalysisErrorDetails() {
    const query = new URLSearchParams(location.search);
    const jobFileId = query.get('jobFileId');
    const jobId = query.get('jobId');
    const type = query.get('type');
    const [loader, setLoader] = useState(false);
    const [errorDate, setErrorDate] = useState('');
    const [filename, setFilename] = useState('');
    const [analysisErrorDetails, setAnalysisErrorDetails] = useState([]);

    useEffect(() => {
        if (jobFileId && jobId && type === 'error') {
            const errorDetailsInfo = JSON.parse(sessionStorage.getItem('errorDetailsInfo')) || {};
            setFilename(errorDetailsInfo.filename || '');
            setErrorDate(errorDetailsInfo.errorDate || '');
            setAnalysisErrorDetails(errorDetailsInfo.errorDetails || []);
        } else {
            getAnalysisErrorsData(jobFileId, jobId);
        }
    }, [jobFileId, jobId, type]);

    const getAnalysisErrorsData = async (jobFileId, jobId) => {
        try {
            setLoader(true);
            const response = await axios.post(ApiHelper.getApiUrl(DvtApiPaths.Home.AnalyzeFilesErrors), {
                jobFileId: jobFileId,
                jobId: jobId
            });
            const { summarizeds, date, fileName } = response.data.data;
            let errorDetails = []
            setErrorDate(date);
            setFilename(fileName);
            summarizeds.forEach(s => {
                errorDetails = errorDetails.concat(s.details);
            });

            setAnalysisErrorDetails(errorDetails);
            setLoader(false);
        } catch (error) {
            console.error("Error fetching analysis data:", error);
        } finally {
            setLoader(false);
        }
    }

    return (
        <div>
            <h4 className={style.analysisTitle}>Emerson Data Validation Tool Error/Warning Report</h4>
            <hr />
            <div className={style.errorDetailsContent}>
                <div className={style.errorDetailsHeader}>
                    <div>
                        <strong>Date:</strong> {errorDate ? convertUTCTimeToLocalTime(errorDate, "MM/DD/YYYY") : ''}
                    </div>
                    <div>
                        <strong>Filename:</strong> {filename}
                    </div>
                </div>
                <div className={style.errorDetailsBody}>
                    {analysisErrorDetails.map((detail, index) => (
                        <div key={index} className={style.errorDetailItem}>
                            <div>
                                <strong>Row Number:</strong> {detail.rowNumber === -1 ? 'N/A' : detail.rowNumber}
                            </div>
                            <div>
                                <strong>Problem:</strong> {detail.problem}
                            </div>
                            <div>
                                <strong>Validation Message:</strong> {detail.errorDescription}
                            </div>
                            <div>
                                <strong>Data:</strong> {detail.data}
                            </div>
                        </div>
                    ))}
                </div>
            </div>
            <React.Suspense fallback={<div>Loading...</div>}>
                {loader && <FullPageLoader />}
            </React.Suspense>
        </div>
    )
}

export default AnalysisErrorDetails
