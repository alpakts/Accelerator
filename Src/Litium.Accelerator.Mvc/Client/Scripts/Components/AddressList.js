import React, { useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { translate } from '../Services/translation';
import { remove } from '../Actions/Address.action';

const AddressList = ({ onEdit }) => {
    const dispatch = useDispatch();
    const addresses = useSelector((state) => state.myPage.addresses.list);

    const [removingRow, setRemovingRow] = useState({});

    const onRemoveRequest = (rowSystemId, showDeleteButton) => {
        setRemovingRow((previousState) => {
            return {
                ...previousState,
                [rowSystemId]: showDeleteButton,
            };
        });
    };

    return (
        <div className="simple-table">
            <div className="row medium-unstack no-margin simple-table__header">
                <div className="columns">
                    {translate('mypage.address.address')}
                </div>
                <div className="columns">
                    {translate('mypage.address.postnumber')}
                </div>
                <div className="columns">
                    {translate('mypage.address.city')}
                </div>
                <div className="columns medium-2 hide-for-small-only"></div>
            </div>

            {addresses &&
                addresses.map((address) => (
                    <div
                        className="row medium-unstack no-margin"
                        key={`${address.systemId}`}
                    >
                        <div className="columns">{address.address}</div>
                        <div className="columns">{address.zipCode}</div>
                        <div className="columns">{address.city}</div>
                        <div className="columns medium-2">
                            <a
                                className="table__icon table__icon--edit"
                                onClick={() => onEdit(address)}
                                title={translate('Edit')}
                            ></a>
                            {!removingRow[address.systemId] && (
                                <a
                                    className="table__icon table__icon--delete"
                                    onClick={() =>
                                        onRemoveRequest(address.systemId, true)
                                    }
                                    title={translate('Remove')}
                                ></a>
                            )}
                            {removingRow[address.systemId] && (
                                <a
                                    className="table__icon table__icon--accept"
                                    onClick={() =>
                                        dispatch(remove(address.systemId))
                                    }
                                    title={translate('Accept')}
                                ></a>
                            )}
                            {removingRow[address.systemId] && (
                                <a
                                    className="table__icon table__icon--cancel"
                                    onClick={() =>
                                        onRemoveRequest(address.systemId, false)
                                    }
                                    title={translate('Cancel')}
                                ></a>
                            )}
                        </div>
                    </div>
                ))}
        </div>
    );
};

export default AddressList;
