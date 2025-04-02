/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApiLinkModel } from './ApiLinkModel';
import type { IApiMediaWithCropsModel } from './IApiMediaWithCropsModel';
export type CalendarEventPropertiesModel = {
    description?: string | null;
    bannerImage?: Array<IApiMediaWithCropsModel> | null;
    eventLocation?: string | null;
    country?: string | null;
    dateFrom?: string | null;
    dateTo?: string | null;
    eventLink?: Array<ApiLinkModel> | null;
    onlineAttendance?: boolean | null;
    tags?: Array<string> | null;
};

