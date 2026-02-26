import { apiClient } from "./apiClient";
import type { Person, PersonDto } from "../types/person";

const PERSON_ENDPOINT = "/person";

export async function getPeople(): Promise<Person[]> {
  const response = await apiClient.get<Person[]>(PERSON_ENDPOINT);
  return response.data;
}

export async function createPerson(personDto: PersonDto): Promise<Person> {
  const response = await apiClient.post<Person>(PERSON_ENDPOINT, personDto);
  return response.data;
}

export async function updatePerson(
  id: string,
  personDto: PersonDto
): Promise<void> {
  await apiClient.put(`${PERSON_ENDPOINT}/${id}`, personDto);
}

export async function deletePerson(id: string): Promise<void> {
  await apiClient.delete(`${PERSON_ENDPOINT}/${id}`);
}
