import React, { useState, useEffect } from 'react';
import { useHistory, useLocation } from "react-router-dom";
import Typography from '@mui/material/Typography';
import BreadcrumbsMui from '@mui/material/Breadcrumbs';
import Link from '@mui/material/Link';
import { BreadcrumbsMapping } from "./BreadcrumbsMapping";
const Breadcrumbs = () => {
    const [breadcrumbs, setBreadcrumbs] = useState([]);
    const location = useLocation();
    let history = useHistory();
    const { key: locationKey } = useLocation()
    const pathnames = location.pathname.split('/').filter((x) => x);
    useEffect(() => {
        let crumbs = []
        pathnames.forEach((value, index) => {
            crumbs.push({
                name: BreadcrumbsMapping['/' + value],
                url: '/' + value
            })
        })
        const isHome = pathnames[0] === 'home';
        if (!isHome) {
            crumbs.unshift({
                name: 'Home',
                url: '/home'
            })
        }
        setBreadcrumbs(crumbs)
    }, [locationKey])

    const handleClick = (event) => {
        const url = event.target.href;
        if (url) {
            const linkUrl = url.substring(url.lastIndexOf('/'), url.length)
            history.push(linkUrl)
        }
        event.preventDefault();
    }

    const isLastOne = (index) => {
        return index === breadcrumbs.length - 1;
    }
    return (
        <div className='bread-crumbs' role="presentation" onClick={handleClick}>
            <BreadcrumbsMui aria-label="breadcrumb">
                {
                    breadcrumbs.map((item, index) => {
                        return (
                            isLastOne(index) ? <Typography color="text.primary">{item.name}</Typography> :
                                <Link underline="hover" color="inherit" href={item.url}>
                                    {item.name}
                                </Link>
                        )
                    })
                }
            </BreadcrumbsMui>
        </div>
    )
}

export default Breadcrumbs;