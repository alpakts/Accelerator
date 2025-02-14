import React, { Fragment, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { translate } from '../Services/translation';
import { remove } from '../Actions/Person.action';

const PersonList = ({ onEdit }) => {
    const dispatch = useDispatch();
    const persons = useSelector((state) => state.myPage.persons.list);

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
                <div className="columns">{translate('mypage.person.name')}</div>
                <div className="columns">
                    {translate('mypage.person.email')}
                </div>
                <div className="columns">
                    {translate('mypage.person.phone')}
                </div>
                <div className="columns">{translate('mypage.person.role')}</div>
                <div className="columns medium-2 hide-for-small-only"></div>
            </div>

            {persons &&
                persons.map((person) => (
                    <div
                        className="row medium-unstack no-margin"
                        key={person.systemId}
                    >
                        <div className="columns">
                            {person.firstName} {person.lastName}
                        </div>
                        <div className="columns">{person.email || ''}</div>
                        <div className="columns">{person.phone || ''}</div>
                        <div className="columns">{person.role}</div>
                        <div className="columns medium-2">
                            {person.editable && (
                                <Fragment>
                                    <a
                                        onClick={() => onEdit(person)}
                                        className="table__icon table__icon--edit"
                                        title={translate('Edit')}
                                    ></a>
                                    {!removingRow[person.systemId] && (
                                        <a
                                            onClick={() =>
                                                onRemoveRequest(
                                                    person.systemId,
                                                    true
                                                )
                                            }
                                            className="table__icon table__icon--delete"
                                            title={translate('Remove')}
                                        ></a>
                                    )}
                                    {removingRow[person.systemId] && (
                                        <a
                                            className="table__icon table__icon--accept"
                                            onClick={() =>
                                                dispatch(
                                                    remove(person.systemId)
                                                )
                                            }
                                            title={translate('Accept')}
                                        ></a>
                                    )}
                                    {removingRow[person.systemId] && (
                                        <a
                                            className="table__icon table__icon--cancel"
                                            onClick={() =>
                                                onRemoveRequest(
                                                    person.systemId,
                                                    false
                                                )
                                            }
                                            title={translate('Cancel')}
                                        ></a>
                                    )}
                                </Fragment>
                            )}
                        </div>
                    </div>
                ))}
        </div>
    );
};

export default PersonList;
