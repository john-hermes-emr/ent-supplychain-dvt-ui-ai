import { useOktaAuth } from '@okta/okta-react';
import React, { useState, useEffect, useContext, useRef } from 'react';
import { useSelector, useDispatch } from 'react-redux'
import { useHistory, useLocation } from 'react-router-dom';
import style from './home.module.css';
import { setUserInfomation, clearUserInfomation } from '../../redux/reducers/userInfoSliceReducer';
import Box from '@mui/material/Box';
import { DataGrid } from '@mui/x-data-grid';
import axios from 'axios';
import { ApiHelper } from '../../dvt_api/ApiHelper';
import { DvtApiPaths } from '../../dvt_api/DvtApiPaths';
import './home.css'
import CustomSelect from '../../components/CustomSelect/CustomSelect';
import CustomInput from '../../components/CustomInput/CustomInput';
import CustomButton from '../../components/CustomButton/CustomButton';
import Constant from '../../utils/constant/index.json';
import { convertUTCTimeToLocalTime } from '../../utils/js/utils';
// import StatusProgress from './StatusProgress';
import { Typography } from '@mui/material';
import ToastifyModal from '../../components/ToastifyModal';


const CustomDialog = React.lazy(() => import('../../components/CustomDialog/CustomDialog'));
const FullPageLoader = React.lazy(() => import('../../components/FullPageLoader/index.jsx'));
const AtLeastOneFileRequired = Constant.Home.AtLeastOneFileRequired;
const ExistingOperation = Constant.Home.ExistingOperation;
const DoYouWantToProceed = Constant.Home.DoYouWantToProceed;
const ExistingOperationNotMatch = Constant.Home.ExistingOperationNotMatch;
const ExistingActiveJob = Constant.Home.ExistingActiveJob;
const DifferentStatusFiles = Constant.Home.DifferentStatusFiles;
const ForceRefreshMessage = Constant.Home.ForceRefreshMessage;
const DivisionOrFeedRequired = Constant.Home.DivisionOrFeedRequired;
const SingleFileRequired = Constant.Home.SingleFileRequired;
const InProgressFileRequired = Constant.Home.InProgressFileRequired;
const NoDataAvailable = Constant.Home.NoDataAvailable;
const LoadAllThenValidate = Constant.Home.LoadAllThenValidate;
const ClearSelection = Constant.Home.ClearSelection;
const UpdateData = Constant.Home.UpdateData;
const DisplayValidationDetails = Constant.Home.DisplayValidationDetails;


const Home = () => {
	const { authState, oktaAuth } = useOktaAuth();
	const history = useHistory();
	const location = useLocation();
	const cameFromAnalysis = location.state && location.state.fromAnalysis;
	const [userInfo, setUserInfo] = useState(null);
	const [loading, setLoading] = useState(false);
	const dispatch = useDispatch();
	const [divisions, setDivisions] = useState([]);
	const [division, setDivision] = useState('');
	const [feed, setFeed] = useState('');
	const [existingJobData, setExistingJobData] = useState(null);
	const [rows, setRows] = useState([]);
	const [selectedIds, setSelectedIds] = useState([]);
	const [openDialog, setOpenDialog] = useState(false);
	const [openConfirmDialog, setOpenConfirmDialog] = useState(false);
	const [confirmDialogContent, setConfirmDialogContent] = useState('');
	const [confirmDialogType, setConfirmDialogType] = useState('');
	const [dialogContent, setDialogContent] = useState('');
	const [loader, setLoader] = useState(false);
	const [userId, setUserId] = useState(null);
	const [currentJobId, setCurrentJobId] = useState(null);
	const [isPending, setIsPending] = useState(false);
	const [noDataMessage, setNoDataMessage] = useState(NoDataAvailable);
	const [statusProgressData, setStatusProgressData] = useState(null);
	const [shouldPoll, setShouldPoll] = useState(false);

	const childRef = useRef();

	const columns = [
		{ field: 'tableName', headerName: 'Table', disableColumnMenu: true, flex: 1, sortable: false },
		{ field: 'fileName', headerName: 'Filename', disableColumnMenu: true, flex: 1, sortable: false },
		{ field: 'fileLastModifiedTimestamp', headerName: 'File Date', disableColumnMenu: true, flex: 1, sortable: false },
		{ field: 'status', headerName: 'Status', disableColumnMenu: true, flex: 1, sortable: false },
		{ field: 'recordCount', headerName: 'Record Count', disableColumnMenu: true, flex: 1, sortable: false },
		{ field: 'loadDate', headerName: 'Load Date', disableColumnMenu: true, flex: 1, sortable: false }
	];
	useEffect(() => {
		if (!authState || !authState.isAuthenticated) {
			// When user isn't authenticated, forget any user info
			setUserInfo(null);
			dispatch(clearUserInfomation())
		} else {
			oktaAuth.getUser().then((info) => {
				setUserInfo(info);
				dispatch(setUserInfomation(info))
				getUserDefaultPaths(info.email);

			});
		}
	}, [authState, oktaAuth]); // Update if authState changes

	useEffect(() => {
		getDivisions();
		if (cameFromAnalysis) {
			const savedDivision = sessionStorage.getItem('divisionId');
			const savedFeed = sessionStorage.getItem('feedNumber');
			if (savedDivision && savedFeed) {
				setDivision(savedDivision);
				setFeed(savedFeed);
			}
		}
	}, []);

	useEffect(() => {
		if (division && feed && userId && !isPending) {
			setIsPending(true);
			setLoader(true);
			createActiveJob({
				divisionId: division,
				feedNumber: Number(feed),
				userInfoId: userId,
			}, "initial").finally(() => setIsPending(false));
		}
	}, [division, feed, userId]);

	useEffect(() => {
		if (statusProgressData && statusProgressData.jobId === currentJobId && statusProgressData.jobFileStatus.length > 0) {
			const { jobFileStatus } = statusProgressData;
			const updateRows = rows.map(row => {
				let updatedFile = jobFileStatus.find(file => file.key === row.id);
				if (updatedFile) {
					updatedFile.status = updatedFile.value;
					return updatedFile ? { ...row, ...updatedFile } : row;
				}
				return row;
			});
			setRows(updateRows);
		}
	}, [statusProgressData]);

	useEffect(() => {
		if (currentJobId && shouldPoll) {
			const interval = setInterval(() => {
				getJobStatus(currentJobId).then(data => {
					const { jobStatus } = data;
					if (jobStatus === 'VALIDATED') {
						clearInterval(interval);
						setShouldPoll(false);
						setLoading(false);
						setSelectedIds([]);
					}
					setStatusProgressData(data);
				}).catch(error => {
					clearInterval(interval);
					setShouldPoll(false);
					setLoading(false);
					setSelectedIds([]);
				});
			}, 5000);
			return () => clearInterval(interval);
		}
	}, [currentJobId, shouldPoll]);

	const getJobStatus = async (jobId) => {
		try {
			const apiUrl = ApiHelper.getApiUrlWithId(DvtApiPaths.Home.GetJobStatus, jobId);
			const response = await axios.get(apiUrl);
			return response.data;
		} catch (error) {
			console.error('Error fetching job status:', error);
			throw error;
		}
	};

	const getDivisions = async () => {
		try {
			const response = await axios.get(ApiHelper.getApiUrl(DvtApiPaths.Home.GetDivisions));
			setDivisions(response.data.map(item => ({ value: item.itemId, label: item.itemName })));
		} catch (error) {
			console.error('Error fetching divisions:', error);
		}
	};

	const getUserDefaultPaths = async (userEmail) => {
		try {
			const apiUrl = ApiHelper.getApiUrlWithId(DvtApiPaths.ChangePath.GetUserDefaultPaths, userEmail);
			const response = await axios.get(apiUrl);
			const apidata = response.data;
			const { userInfoId } = apidata;
			setUserId(userInfoId);
		} catch (error) {
			console.error('Error fetching user default paths:', error);
		}
	}

	const renderRows = (data) => {
		const { jobFiles, jobId } = data;
		setRows(
			jobFiles.map
				(
					item => ({
						id: item.jobFileId, ...item
						, fileLastModifiedTimestamp: convertUTCTimeToLocalTime(item.fileLastModifiedTimestamp)
						, loadDate: item.loadDate ? convertUTCTimeToLocalTime(item.loadDate) : ''
					})
				).filter(item => item.status !== 'COMPLETED')
				.sort((a, b) => a.sortOrder - b.sortOrder)
		);
		setCurrentJobId(jobId);
	}

	const createActiveJob = async (createJobData, requestType) => {
		try {
			setLoader(true);
			setRows([]);
			const response = await axios.post(ApiHelper.getApiUrl(DvtApiPaths.Home.CreateActiveJob), createJobData);
			const { message, success, data } = response.data;
			if (success) {
				renderRows(data);
			} else if (message === ExistingOperationNotMatch) {
				const { divisionId, feedNumber } = data;
				if (['forceCreate', 'initial'].includes(requestType)) {
					setExistingJobData(data);
					showExistingOperationDialog(divisionId, feedNumber);
				}
			} else if (message === ExistingActiveJob) {
				renderRows(data);
			} else {
				setNoDataMessage(message);
				setRows([]);
			}
			setLoader(false);
		} catch (error) {
			setLoader(false);
			console.error('Error creating active job:', error);
			throw error;
		}
	};

	const showExistingOperationDialog = (divisionId, feedNumber) => {
		const divisionName = divisions.find(item => item.value === divisionId)?.label || '';
		const existingMessage = ExistingOperation.replace('{id}', divisionName).replace('{id}', feedNumber)
		const html = (
			<div className='existing-content'>
				<div>{existingMessage}</div>
				<div>{DoYouWantToProceed}</div>
			</div>
		)
		setConfirmDialogContent(html);
		setOpenConfirmDialog(true);
		setConfirmDialogType('existingJob');
	}

	const handleLoadExtractFiles = async () => {
		if (selectedIds.length === 0) {
			setDialogContent(AtLeastOneFileRequired);
			setOpenDialog(true);
			return;
		}
		const selectedStatuses = rows.filter(row => selectedIds.includes(row.id)).map(row => row.status);
		const isDifferentStatus = selectedStatuses.some(status => status !== selectedStatuses[0]);
		if (isDifferentStatus) {
			const html = (
				<div className='different-status-content'>
					<div>{DifferentStatusFiles}</div>
				</div>
			)
			setConfirmDialogContent(html);
			setOpenConfirmDialog(true);
			setConfirmDialogType('differentStatus');
			return;
		}
		loadJobFiles(currentJobId)
	}

	const validateJobFiles = async (jobId, selectedFileIds) => {
		try {
			const response = await axios.post(ApiHelper.getApiUrl(DvtApiPaths.Home.ValidateJobFiles),
				{
					jobId,
					selectedFileIds
				});
			renderRows(response.data.data);
		} catch (error) {
			setShouldPoll(false);
			setLoading(false);
			setSelectedIds([]);
			console.error('Error validating job files:', error);
			throw error;
		}
	};

	const loadJobFiles = async (jobId) => {
		try {
			setLoader(true);
			setShouldPoll(true);
			setLoading(true);
			setTimeout(() => { setLoader(false); }, 500);
			const response = await axios.post(ApiHelper.getApiUrl(DvtApiPaths.Home.LoadJobFiles),
				{
					jobId
				});
			// setLoader(false);
			const { data } = response.data;
			if (data) {
				renderRows(response.data.data);
			}
			validateJobFiles(jobId, selectedIds);
		} catch (error) {
			// setLoader(false);
			console.error('Error loading job files:', error);
			throw error;
		}
	};

	const handleRefresh = () => {
		if (!division || !feed) {
			setDialogContent(DivisionOrFeedRequired);
			setOpenDialog(true);
			return;
		}
		const html = (
			<div className='fresh-content'>
				<div>{ForceRefreshMessage}</div>
			</div>
		)
		setConfirmDialogContent(html);
		setOpenConfirmDialog(true);
		setConfirmDialogType('refreshJob');
	}

	const handleAnalysis = () => {
		if (selectedIds.length === 0) {
			setDialogContent(AtLeastOneFileRequired);
			setOpenDialog(true);
			return;
		}

		if (selectedIds.length > 1) {
			setDialogContent(SingleFileRequired);
			setOpenDialog(true);
			return;
		}
		const analysisStatuses = ['VALIDATED', 'CRITICAL', 'ERRORS', 'WARNING'];
		const selectedData = rows.filter(row => selectedIds.includes(row.id));
		const selectedStatuses = selectedData.map(row => row.status);
		const selectedFileTypes = selectedData.map(row => row.fileType);
		if (!selectedStatuses.every(status => analysisStatuses.includes(status))) {
			setDialogContent(InProgressFileRequired);
			setOpenDialog(true);
			return;
		}
		sessionStorage.setItem('divisionId', division);
		sessionStorage.setItem('feedNumber', feed);
		history.push({
			pathname: '/analysis',
			search: `?jobFileId=${selectedIds[0]}&jobId=${currentJobId}&fileType=${selectedFileTypes[0]}&status=${selectedStatuses[0]}`,
		});
	}

	const handleClearData = () => {
		setDivision('');
		setFeed('');
		setRows([]);
		setSelectedIds([]);
	}
	if (!authState) {
		return <div>Loading...</div>;
	}

	const NoRowsOverlay = () => (
		<Box
			sx={{
				height: '100%',
				display: 'flex',
				alignItems: 'center',
				justifyContent: 'center',
				flexDirection: 'column',
			}}
		>
			<Typography variant="body1" color="textSecondary">
				{noDataMessage}
			</Typography>
		</Box>
	);

	return (
		<div className='homeContainer'>
			<form className='homeContainerForm'>
				<div className={style.rowContainer}>
					<div className={`${style.item} ${style.divisionSelect}`}>
						<label className={style.label} htmlFor="division-select">Div:</label>
						<CustomSelect
							value={division}
							options={divisions}
							onChange={(data) => {
								setDivision(data ? data : '');
							}}
							placeholder="Select Division"
						/>
					</div>
					<div className={style.item}>
						<label className={style.label} htmlFor="feed-input">Feed:</label>
						<CustomInput
							name="feed"
							type='number'
							max={99}
							min={1}
							value={feed}
							onChange={(e) => {
								if (!isPending) {
									setFeed(e.target.value);
								}
							}}
						/>
					</div>
					<div className={style.item}>
						<CustomButton
							title={ClearSelection}
							children="Clear Data"
							onClick={handleClearData}
						/>
					</div>
					<div className={style.item}>
						<CustomButton
							title={UpdateData}
							children="Refresh"
							onClick={handleRefresh}
						/>
					</div>
					<div className={style.item}>
						<CustomButton
							title={LoadAllThenValidate}
							children="Load Extract Files"
							onClick={handleLoadExtractFiles}
						/>
					</div>
					<div className={style.item}>
						<CustomButton
							title={DisplayValidationDetails}
							children="Analysis"
							onClick={handleAnalysis}
						/>
					</div>
				</div>
			</form>

			{/* <StatusProgress
				jobId={currentJobId}
				statusProgressData={statusProgressData}
				setStatusProgressData={setStatusProgressData}
			/> */}

			<Box className="homeDataGrid" sx={{ height: 550, width: '100%' }}>
				<DataGrid
					rows={rows}
					rowHeight={45}
					columns={columns}
					loading={loading}
					initialState={{
						pagination: {
							paginationModel: {
								pageSize: 15,
							},
						},
					}}
					rowCount={rows.length}
					disableRowSelectionOnClick
					disableSelectionOnClick
					disableColumnFilter
					disableColumnSelector
					checkboxSelection
					rowSelectionModel={selectedIds}
					onRowSelectionModelChange={(ids) => setSelectedIds(ids)}
					slots={{
						noRowsOverlay: NoRowsOverlay,
					}}
				/>
			</Box>
			<React.Suspense fallback={<div>Loading...</div>}>
				<CustomDialog
					open={openDialog}
					handleClose={() => setOpenDialog(false)}
					content={dialogContent}
					actions={[
						{
							label: 'OK',
							onClick: () => setOpenDialog(false),
						}
					]}
					customWidth={300}
					className="customDialog"
				/>
				<CustomDialog
					title={confirmDialogType === 'existingJob' ? 'Existing Operation' : ''}
					open={openConfirmDialog}
					handleClose={() => setOpenConfirmDialog(false)}
					htmlContent={confirmDialogContent}
					actions={[
						{
							label: confirmDialogType === 'existingJob' ? 'Resume' : 'OK',
							onClick: () => {
								if (confirmDialogType === 'existingJob') {
									const { divisionId, feedNumber } = existingJobData;
									setDivision(divisionId);
									setFeed(feedNumber);
								} else if (confirmDialogType === 'differentStatus') {
									loadJobFiles(currentJobId)
								} else if (confirmDialogType === 'refreshJob') {
									createActiveJob({
										"divisionId": division,
										"feedNumber": Number(feed),
										"userInfoId": userId,
										"ForceCreate": true
									}, "forceCreate");
								}
								setOpenConfirmDialog(false);
							},
						},
						{
							label: 'Cancel',
							onClick: () => {
								if (confirmDialogType === 'existingJob') {
									createActiveJob({
										"divisionId": division,
										"feedNumber": Number(feed),
										"userInfoId": userId,
										"ForceCreate": true
									}, 'forceCreate');
								}
								setOpenConfirmDialog(false);
							},
						}
					]}
					customWidth={confirmDialogType === 'existingJob' ? 480 : 300}
					className="customDialogExisting"
				/>
				<ToastifyModal ref={childRef} />
				{loader && <FullPageLoader />}
			</React.Suspense>
		</div>
	);
};
export default Home;