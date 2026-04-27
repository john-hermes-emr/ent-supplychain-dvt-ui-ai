import React, { useState, useEffect, useRef } from 'react'
import { useSelector, useDispatch } from 'react-redux'
import { useHistory } from 'react-router-dom'
import style from './analysis.module.css'
import CustomButton from '../../components/CustomButton/CustomButton'
import AnalysisStatistics from './AnalysisStatistics';
import AnalysisErrors from './AnalysisErrors';
import helpIcon from '../../../asset/help-icon.svg';
import Constant from '../../utils/constant/index.json';
import axios from 'axios';
import { ApiHelper } from '../../dvt_api/ApiHelper';
import { DvtApiPaths } from '../../dvt_api/DvtApiPaths';
import { saveAs } from 'file-saver';
import ToastifyModal from '../../components/ToastifyModal';
import './analysis.css';

const viewErrorsTitle = Constant.Analysis.viewErrorsTitle;
const RecordsContainWarningsOrErrors = Constant.Home.RecordsContainWarningsOrErrors;
const fileTypes = [
    {
        name: 'VIR',
        value: 'Vir'
    },
    {
        name: 'INV',
        value: 'Inventory'
    },
    {
        name: 'PO',
        value: 'Po'
    },
    {
        name: 'POITEM',
        value: 'Poitem'
    },
    {
        name: 'ITEM',
        value: 'Item'
    },
    {
        name: 'SUPPLIER',
        value: 'Supplier'
    },
    {
        name: 'MPN',
        value: 'Mpn'
    },
    {
        name: 'UOM',
        value: 'Uom'
    }
]

const FullPageLoader = React.lazy(() => import('../../components/FullPageLoader/index.jsx'));
const CustomDialog = React.lazy(() => import('../../components/CustomDialog/CustomDialog'));


function Analysis() {
    const history = useHistory();
    const [analysisType, setAnalysisType] = useState('statistics');
    const query = new URLSearchParams(location.search);
    const jobFileId = query.get('jobFileId');
    const jobId = query.get('jobId');
    const fileType = query.get('fileType');
    const status = query.get('status');
    const [analysisErrors, setAnalysisErrors] = useState([]);
    const [analysisStatistics, setAnalysisStatistics] = useState({});
    const [filename, setFilename] = useState('');
    const [errorDate, setErrorDate] = useState('');
    const [loader, setLoader] = React.useState(false);
    const [dialogOpen, setDialogOpen] = useState(false);
    const userEmail = useSelector(state => state.userInfo.email);
    const childRef = useRef();


    useEffect(() => {
        if (jobFileId && jobId) {
            getAnalysisErrorsData(jobFileId, jobId);
            getAnalysisStatisticsData(jobId);
        }
    }, []);

    const getAnalysisErrorsData = async (jobFileId, jobId) => {
        try {
            const response = await axios.post(ApiHelper.getApiUrl(DvtApiPaths.Home.AnalyzeFilesErrors), {
                jobFileId: jobFileId,
                jobId: jobId
            });
            const { summarizeds, fileName, date } = response.data.data;
            setAnalysisErrors(summarizeds || []);
            setFilename(fileName);
            setErrorDate(date);
        } catch (error) {
            console.error("Error fetching analysis data:", error);
        }
    }

    const getAnalysisStatisticsData = async (jobId) => {
        try {
            const response = await axios.post(ApiHelper.getApiUrl(DvtApiPaths.Home.AnalyzeFilesStatisticsSingle), {
                jobFileId: jobFileId,
                jobId: jobId
            });
            const stats = response.data.data.stats;
            if (stats) {
                setAnalysisStatistics(stats);
            }
        } catch (error) {
            console.error("Error fetching analysis statistics:", error);
        }
    }

    const handleSaveReport = async () => {
        try {
            setLoader(true);
            const exportResultsToExcelUrl = DvtApiPaths.Home.AnalyzeFilesErrorsReportExport;
            const params = {
                jobFileId: jobFileId,
                jobId: jobId
            };
            const response = await axios({
                method: 'post',
                url: ApiHelper.getApiUrl(exportResultsToExcelUrl),
                data: params,
                responseType: 'blob'
            });
            // Extract filename from Content-Disposition header
            const disposition = response.headers['content-disposition'];
            let filename = '';
            if (disposition && disposition.indexOf('filename=') !== -1) {
                filename = disposition
                    .split('filename=')[1].split(';')[0]
                    .replace(/['"]/g, '')
                    .trim();
            }
            let url = window.URL
                .createObjectURL(new Blob([response.data]));
            saveAs(url, filename);
            setLoader(false);
        } catch (error) {
            setLoader(false);
            console.error("Error fetching analysis statistics:", error);
        }
    }

    const handleAccept = async () => {
        try {
            setLoader(true);
            const response = await axios.post(ApiHelper.getApiUrl(DvtApiPaths.Home.AcceptValidatedData), {
                jobFileId: jobFileId,
                jobId: jobId,
                updateBy: userEmail
            });
            const { status } = response.data.data;
            const isCompletedJob = status === 'COMPLETED';
            setTimeout(() => {
                history.push('/home', { fromAnalysis: !isCompletedJob });
            }, 2000);
            childRef.current.getConfirmationMessage(response.data.message, "success");
            setLoader(false);
        } catch (error) {
            setLoader(false);
            console.error("Error accepting analyzed file:", error);
        }
    }

    const handleDialogClose = () => {
        setDialogOpen(false);
    }

    return (
        <div>
            <h4 className={style.analysisTitle}>Analysis</h4>
            <hr />
            <div className={style.analysisContainer}>
                <div className={style.buttonContainer}>
                    <CustomButton
                        children="Statistics"
                        color={analysisType === 'statistics' ? 'primary' : 'info'}
                        onClick={() => {
                            setAnalysisType('statistics');
                            // Logic to fetch and display statistics
                            console.log("Fetching statistics...");
                        }}
                    />
                    <CustomButton
                        children="Errors"
                        color={analysisType === 'errors' ? 'primary' : 'info'}
                        onClick={() => {
                            setAnalysisType('errors');
                        }}
                    />
                    {
                        ['VALIDATED', 'ERRORS', 'WARNING'].includes(status) &&
                        <CustomButton
                            children="Accept"
                            onClick={status === 'VALIDATED' ? handleAccept : () => setDialogOpen(true)}
                        />
                    }
                </div>
                <h4 className={style.analysisTitle}>
                    {analysisType === 'statistics' ? 'Statistics' : 'Errors'}
                    {
                        analysisType === 'statistics' &&
                        <div className={style.fileTypeButtonContainer}>
                            {
                                fileTypes.map((type) => (
                                    <CustomButton
                                        color={type.value.toLowerCase() === fileType.toLowerCase() ? 'primary' : 'info'}
                                        key={type.value}
                                        children={type.name}
                                    />
                                ))
                            }
                        </div>

                    }
                    {
                        analysisType === 'errors' &&
                        <div className={style.helpIconContainer}>
                            <img
                                src={helpIcon}
                                alt="Help"
                                className={style.helpIcon}
                            />
                            <span>{viewErrorsTitle}</span>
                        </div>
                    }
                </h4>
                <hr />
                <div className={style.analysisContent}>
                    <div className={style.analysisData}>
                        {analysisType === 'statistics' ? (
                            <AnalysisStatistics
                                statistics={analysisStatistics}
                                fileType={fileType}
                            />
                        ) : (
                            <AnalysisErrors
                                errors={analysisErrors}
                                jobId={jobId}
                                jobFileId={jobFileId}
                                errorDate={errorDate}
                                filename={filename}
                            />
                        )}
                    </div>
                    {
                        analysisType === 'errors' && status !== 'VALIDATED' && <div className={style.reportContainer}>
                            <CustomButton
                                children="Report"
                                onClick={() => {
                                    window.open(`/analysisErrorDetails?jobFileId=${jobFileId}&jobId=${jobId}&type=errors`, '_blank');
                                }}
                            />
                            <CustomButton
                                children="Save Report"
                                onClick={handleSaveReport}
                            />
                        </div>
                    }
                </div>
            </div>
            <ToastifyModal ref={childRef} />
            <React.Suspense fallback={<div>Loading...</div>}>
                <CustomDialog
                    className='acceptDialogOpen'
                    open={dialogOpen}
                    handleClose={handleDialogClose}
                    content={RecordsContainWarningsOrErrors}
                    actions={[
                        {
                            label: 'OK', onClick: () => {
                                handleAccept();
                                handleDialogClose();
                            }
                        },
                        { label: 'Cancel', onClick: handleDialogClose }
                    ]}
                />
                {loader && <FullPageLoader />}
            </React.Suspense>
        </div>
    )
}

export default Analysis

