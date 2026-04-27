import React, { useState } from 'react';
import style from './help.module.css';

export default function Help() {
    return (
        <div className={style.helpContainer}>
            <h4 className={style.helpTitle}>Help</h4>
            <hr />
        </div>
    );
}
